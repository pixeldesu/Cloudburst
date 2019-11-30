using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;

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
        private readonly List<PingIndicator> _pingIndicators = new List<PingIndicator>();

        public PingerController(Pingprovements plugin)
        {
            _config = plugin.GetConfig();

            _colors.Add("DefaultPingColor", _config.DefaultPingColorConfig.Value.ToColor());
            _colors.Add("DefaultPingSpriteColor", _config.DefaultPingSpriteColorConfig.Value.ToColor());
            _colors.Add("EnemyPingColor", _config.EnemyPingColorConfig.Value.ToColor());
            _colors.Add("EnemyPingSpriteColor", _config.EnemyPingSpriteColorConfig.Value.ToColor());
            _colors.Add("InteractiblePingColor", _config.InteractiblePingColorConfig.Value.ToColor());
            _colors.Add("InteractiblePingSpriteColor", _config.InteractiblePingSpriteColorConfig.Value.ToColor());
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
            // This is here to avoid stacking of different player pings on interactibles
            if (newPingInfo.targetGameObject != null &&
                _pingIndicators.Any(indicator => indicator && indicator.pingTarget == newPingInfo.targetGameObject))
                return;

            self.NetworkcurrentPing = newPingInfo;

            // Here we create an instance of PingIndicator
            // since we're not jumping into PingerController.RebuildPing() to create one.
            GameObject go = (GameObject) Object.Instantiate(Resources.Load("Prefabs/PingIndicator"));
            PingIndicator pingIndicator = go.GetComponent<PingIndicator>();

            pingIndicator.pingOwner = self.gameObject;
            pingIndicator.pingOrigin = newPingInfo.origin;
            pingIndicator.pingNormal = newPingInfo.normal;
            pingIndicator.pingTarget = newPingInfo.targetGameObject;

            pingIndicator.RebuildPing();

            SetPingIndicatorColor(pingIndicator);

            float fixedTimer = 0f;

            PingIndicator.PingType pingType = pingIndicator.GetObjectValue<PingIndicator.PingType>("pingType");

            switch (pingType)
            {
                case PingIndicator.PingType.Default:
                    fixedTimer = _config.DefaultPingLifetime.Value;
                    break;
                case PingIndicator.PingType.Enemy:
                    fixedTimer = _config.EnemyPingLifetime.Value;
                    break;
                case PingIndicator.PingType.Interactable:
                    fixedTimer = _config.InteractiblePingLifetime.Value;
                    AddLootText(pingIndicator);
                    break;
            }

            pingIndicator.SetObjectValue("fixedTimer", fixedTimer);

            // We add the ping indicator to our own local list
            _pingIndicators.Add(pingIndicator);

            if (self.hasAuthority)
            {
                self.CallCmdPing(self.currentPing);
            }
        }

        private void SetPingIndicatorColor(PingIndicator pingIndicator)
        {
            SpriteRenderer sprRenderer;
            Color textColor = new Color(0, 0, 0, 0);

            PingIndicator.PingType pingType = pingIndicator.GetObjectValue<PingIndicator.PingType>("pingType");

            switch (pingType)
            {
                case PingIndicator.PingType.Default:
                    textColor = _colors["DefaultPingColor"];
                    sprRenderer = pingIndicator.defaultPingGameObjects[0].GetComponent<SpriteRenderer>();
                    sprRenderer.color = _colors["DefaultPingSpriteColor"];
                    break;
                case PingIndicator.PingType.Enemy:
                    textColor = _colors["EnemyPingColor"];
                    sprRenderer = pingIndicator.enemyPingGameObjects[0].GetComponent<SpriteRenderer>();
                    sprRenderer.color = _colors["EnemyPingSpriteColor"];
                    break;
                case PingIndicator.PingType.Interactable:
                    textColor = _colors["InteractiblePingColor"];
                    sprRenderer = pingIndicator.interactablePingGameObjects[0].GetComponent<SpriteRenderer>();
                    sprRenderer.color = _colors["InteractiblePingSpriteColor"];
                    break;
            }

            pingIndicator.pingText.color = textColor;
        }

        private static void AddLootText(PingIndicator pingIndicator)
        {
            const string textStart = "<size=70%>\n";
            string price = GetPrice(pingIndicator.pingTarget);
            ShopTerminalBehavior shopTerminal = pingIndicator.pingTarget.GetComponent<ShopTerminalBehavior>();
            if (shopTerminal && _config.ShowShopText.Value)
            {
                string text = textStart;
                PickupIndex pickupIndex = shopTerminal.CurrentPickupIndex();
                PickupDef pickup = PickupCatalog.GetPickupDef(pickupIndex);
                text += shopTerminal.pickupIndexIsHidden
                    ? "?"
                    : $"{Language.GetString(pickup.nameToken)}";
                pingIndicator.pingText.text += $"{text} ({price})";
                return;
            }

            GenericPickupController pickupController = pingIndicator.pingTarget.GetComponent<GenericPickupController>();
            if (pickupController && _config.ShowPickupText.Value)
            {
                PickupDef pickup = PickupCatalog.GetPickupDef(pickupController.pickupIndex);
                pingIndicator.pingText.text += $"{textStart}{Language.GetString(pickup.nameToken)}";
            }

            ChestBehavior chest = pingIndicator.pingTarget.GetComponent<ChestBehavior>();
            if (chest && _config.ShowChestText.Value)
            {
                pingIndicator.pingText.text += $"{textStart}{Util.GetBestBodyName(pingIndicator.pingTarget)} ({price})";
                return;
            }

            string name =
                Language.GetString(pingIndicator.pingTarget.GetComponent<PurchaseInteraction>().displayNameToken);

            // Drones
            SummonMasterBehavior summonMaster = pingIndicator.pingTarget.GetComponent<SummonMasterBehavior>();
            if (summonMaster && _config.ShowDroneText.Value)
            {
                pingIndicator.pingText.text += $"{textStart}{name} ({price})";
                return;
            }

            if (_config.ShowShrineText.Value) pingIndicator.pingText.text += $"{textStart}{name}";
        }

        private static string GetPrice(GameObject go)
        {
            PurchaseInteraction purchaseInteraction = go.GetComponent<PurchaseInteraction>();
            if (purchaseInteraction)
            {
                StringBuilder sb = new StringBuilder();
                CostTypeCatalog.GetCostTypeDef(purchaseInteraction.costType)
                    .BuildCostStringStyled(purchaseInteraction.cost, sb, true);

                return sb.ToString();
            }

            return null;
        }
    }
}