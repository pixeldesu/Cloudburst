﻿using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;
using RoR2.UI;

namespace Pingprovements
{
    class PingerController
    {
        /// <summary>
        /// Configuration instance
        /// </summary>
        private static PingprovementsConfig _config;

        /// <summary>
        /// Color instances used for the <see cref="PingIndicator"/>s
        /// </summary>
        private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>();

        /// <summary>
        /// As <see cref="PingerController"/> only can hold one instance of <see cref="PingIndicator"/>, we need to
        /// add our own storage for them. This holds all <see cref="PingIndicator"/>s of the current stage.
        /// </summary>
        private readonly List<RoR2.UI.PingIndicator> _pingIndicators = new List<RoR2.UI.PingIndicator>();

        /// <summary>
        /// PingPainter instance used by the PingerController
        /// </summary>
        private static PingPainter _painter;

        /// <summary>
        /// PingTextBuilder instance used by the PingerController
        /// </summary>
        private static PingTextBuilder _textBuilder;

        public PingerController(Pingprovements plugin)
        {
            _config = plugin.GetConfig();
            _painter = new PingPainter(_config);
            _textBuilder = new PingTextBuilder(_config);
        }

        /// <summary>
        /// If a scene unloads, all the <see cref="PingIndicator"/> instances become invalidated by the game. 
        /// To save on memory we also clear out the local list of <see cref="PingIndicator"/>s we carry!
        /// </summary>
        public void OnSceneUnloaded(Scene scene)
        {
            _pingIndicators.Clear();
        }

        /// <summary>
        /// Override method for RoR2.PingerController.SetCurrentPing
        /// </summary>
        public void SetCurrentPing(On.RoR2.PingerController.orig_SetCurrentPing orig,
            RoR2.PingerController self, RoR2.PingerController.PingInfo newPingInfo)
        {
            // For some reason, if you ping somewhere that is not pingable, it will create a
            // Ping at 0,0,0. If that happens, we just leave, since that isn't possible in the
            // regular game either, or if so, not at exactly those coordinates
            if (newPingInfo.origin == Vector3.zero)
                return;

            // If the targeted game object already has a ping, don't do anything
            // This is here to avoid stacking of different player pings on interactables
            if (newPingInfo.targetGameObject != null &&
                _pingIndicators.Any(indicator => indicator && indicator.pingTarget == newPingInfo.targetGameObject))
                return;

            self.NetworkcurrentPing = newPingInfo;

            // Here we create an instance of PingIndicator
            // since we're not jumping into PingerController.RebuildPing() to create one.
            GameObject go = (GameObject) Object.Instantiate(Resources.Load("Prefabs/PingIndicator"));
            RoR2.UI.PingIndicator pingIndicator = go.GetComponent<RoR2.UI.PingIndicator>();

            pingIndicator.pingOwner = self.gameObject;
            pingIndicator.pingOrigin = newPingInfo.origin;
            pingIndicator.pingNormal = newPingInfo.normal;
            pingIndicator.pingTarget = newPingInfo.targetGameObject;

            pingIndicator.RebuildPing();
            
            RoR2.UI.PingIndicator.PingType pingType =
                pingIndicator.GetObjectValue<RoR2.UI.PingIndicator.PingType>("pingType");

            _painter.SetPingIndicatorColor(pingIndicator, pingType);
            _textBuilder.SetPingText(pingIndicator, pingType);
            SetPingTimer(pingIndicator, pingType);

            if (pingType == RoR2.UI.PingIndicator.PingType.Interactable)
            {
                ShowUnlockedItemNotification(pingIndicator);
            }

            // We add the ping indicator to our own local list
            _pingIndicators.Add(pingIndicator);

            if (self.hasAuthority)
            {
                self.CallCmdPing(self.currentPing);
            }
        }

        private void SetPingTimer(RoR2.UI.PingIndicator pingIndicator, RoR2.UI.PingIndicator.PingType pingType)
        {
            float fixedTimer = 0f;

            switch (pingType)
            {
                case RoR2.UI.PingIndicator.PingType.Default:
                    fixedTimer = _config.DefaultPingLifetime.Value;
                    break;
                case RoR2.UI.PingIndicator.PingType.Enemy:
                    fixedTimer = _config.EnemyPingLifetime.Value;
                    break;
                case RoR2.UI.PingIndicator.PingType.Interactable:
                    fixedTimer = _config.InteractablePingLifetime.Value;
                    break;
            }

            pingIndicator.SetObjectValue("fixedTimer", fixedTimer);
        }

        private void ShowUnlockedItemNotification(RoR2.UI.PingIndicator pingIndicator)
        {
            GenericPickupController pickupController = pingIndicator.pingTarget.GetComponent<GenericPickupController>();
            if (pickupController && _config.ShowItemNotification.Value)
            {
                BuildNotification(pickupController.pickupIndex, pingIndicator);
            }

            PurchaseInteraction purchaseInteraction = pingIndicator.pingTarget.GetComponent<PurchaseInteraction>();
            if (purchaseInteraction && _config.ShowItemNotification.Value)
            {
                ShopTerminalBehavior shopTerminalBehavior = purchaseInteraction.GetComponent<ShopTerminalBehavior>();
                if (shopTerminalBehavior && !shopTerminalBehavior.pickupIndexIsHidden)
                {
                    BuildNotification(shopTerminalBehavior.CurrentPickupIndex(), pingIndicator);
                }
            }
        }

        private void BuildNotification(PickupIndex pickupIndex, RoR2.UI.PingIndicator pingIndicator)
        {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (localUser.currentNetworkUser.userName ==
                Util.GetBestMasterName(pingIndicator.pingOwner.GetComponent<CharacterMaster>()))
            {
                if (localUser.userProfile.HasDiscoveredPickup(pickupIndex))
                {
                    PickupDef pickup = PickupCatalog.GetPickupDef(pickupIndex);
                    
                    ItemDef item = ItemCatalog.GetItemDef(pickup.itemIndex);
                    EquipmentDef equip = EquipmentCatalog.GetEquipmentDef(pickup.equipmentIndex);
                    ArtifactDef artifact = ArtifactCatalog.GetArtifactDef(pickup.artifactIndex);

                    GenericNotification notification = Object
                        .Instantiate(Resources.Load<GameObject>("Prefabs/NotificationPanel2"))
                        .GetComponent<GenericNotification>();

                    if (item != null)
                    {
                        notification.SetItem(item);
                    }
                    else if (equip != null)
                    {
                        notification.SetEquipment(equip);
                    }
                    else if (artifact != null)
                    {
                        notification.SetArtifact(artifact);
                    }

                    notification.transform.SetParent(RoR2Application.instance.mainCanvas.transform, false);
                    notification.transform.position = new Vector3(notification.transform.position.x + 2, 95, 0);
                }
            }
        }
    }
}