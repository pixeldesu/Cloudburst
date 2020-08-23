using RoR2;
using RoR2.UI;
using UnityEngine;

namespace Pingprovements
{
    public class PingNotificationBuilder
    {
        private PingprovementsConfig _config;

        public PingNotificationBuilder(PingprovementsConfig config)
        {
            _config = config;
        }
        
        public void SetUnlockedItemNotification(RoR2.UI.PingIndicator pingIndicator)
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