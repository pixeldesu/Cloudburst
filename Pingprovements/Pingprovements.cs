using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;

namespace Pingprovements
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.pixeldesu.pingprovements", "Pingprovements", "1.2.0")]
    public class Pingprovements : BaseUnityPlugin
    {
        #region Private Fields
        // Config variables for ping lifetimes
        private static ConfigWrapper<int> DefaultPingLifetime { get; set; }
        private static ConfigWrapper<int> EnemyPingLifetime { get; set; }
        private static ConfigWrapper<int> InteractiblePingLifetime { get; set; }


        // Config variables for ping text colors
        private static ConfigWrapper<string> DefaultPingColorConfig { get; set; }
        private static ConfigWrapper<string> EnemyPingColorConfig { get; set; }
        private static ConfigWrapper<string> InteractiblePingColorConfig { get; set; }


        // Config variables for ping sprite colors
        private static ConfigWrapper<string> DefaultPingSpriteColorConfig { get; set; }
        private static ConfigWrapper<string> EnemyPingSpriteColorConfig { get; set; }
        private static ConfigWrapper<string> InteractiblePingSpriteColorConfig { get; set; }
        
        // Config variables for interactible additional text
        private static ConfigWrapper<bool> ShowShopText { get; set; }
        private static ConfigWrapper<bool> ShowChestText { get; set; }
        private static ConfigWrapper<bool> ShowPickupText { get; set; }
        private static ConfigWrapper<bool> ShowDroneText { get; set; }
        private static ConfigWrapper<bool> ShowShrineText { get; set; }

        // Dictionary containing all color definitions
        private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>();

        /**
         * <summary>
         *      As <see cref="PingerController"/> only can hold one instance of <see cref="PingIndicator"/>, we need to
         *      add our own storage for them. This holds all <see cref="PingIndicator"/>s of the current stage
         * </summary>
         */
        private readonly List<PingIndicator> _pingIndicators = new List<PingIndicator>();
        #endregion

        #region Configuration and Startup
        public void Awake()
        {
            DefaultPingLifetime = Config.Wrap(
                "Durations",
                "DefaultPingLifetime",
                "Time in seconds how long a regular 'walk to' ping indicator should be shown on the map",
                6
            );

            EnemyPingLifetime = Config.Wrap(
                "Durations",
                "EnemyPingLifetime",
                "Time in seconds how long a ping indicator for enemies should be shown on the map",
                8
            );

            InteractiblePingLifetime = Config.Wrap(
                "Durations",
                "InteractiblePingLifetime",
                "Time in seconds how long a ping indicator for interactibles should be shown on the map",
                30
            );

            DefaultPingColorConfig = Config.Wrap(
                "Colors",
                "DefaultPingColor",
                "Color of the default ping, in UnityEngine.Color R/G/B/A Float format",
                "0.525,0.961,0.486,1.000"
            );

            DefaultPingSpriteColorConfig = Config.Wrap(
                "SpriteColors",
                "DefaultPingSpriteColor",
                "Color of the default ping sprite, in UnityEngine.Color R/G/B/A Float format",
                "0.527,0.962,0.486,1.000"
            );

            _colors.Add("DefaultPingColor", DefaultPingColorConfig.Value.ToColor());
            _colors.Add("DefaultPingSpriteColor", DefaultPingSpriteColorConfig.Value.ToColor());

            EnemyPingColorConfig = Config.Wrap(
                "Colors",
                "EnemyPingColor",
                "Color of the enemy ping, in UnityEngine.Color R/G/B/A Float format",
                "0.820,0.122,0.122,1.000"
            );

            EnemyPingSpriteColorConfig = Config.Wrap(
                "SpriteColors",
                "EnemyPingSpriteColor",
                "Color of the enemy ping sprite, in UnityEngine.Color R/G/B/A Float format",
                "0.821,0.120,0.120,1.000"
            );

            _colors.Add("EnemyPingColor", EnemyPingColorConfig.Value.ToColor());
            _colors.Add("EnemyPingSpriteColor", EnemyPingSpriteColorConfig.Value.ToColor());

            InteractiblePingColorConfig = Config.Wrap(
                "Colors",
                "InteractiblePingColor",
                "Color of the interactible ping, in UnityEngine.Color R/G/B/A Float format",
                "0.886,0.871,0.173,1.000"
            );

            InteractiblePingSpriteColorConfig = Config.Wrap(
                "SpriteColors",
                "InteractiblePingSpriteColor",
                "Color of the interactible ping sprite, in UnityEngine.Color R/G/B/A Float format",
                "0.887,0.870,0.172,1.000"
            );

            _colors.Add("InteractiblePingColor", InteractiblePingColorConfig.Value.ToColor());
            _colors.Add("InteractiblePingSpriteColor", InteractiblePingSpriteColorConfig.Value.ToColor());

            ShowPickupText = Config.Wrap(
                "ShowPingText",
                "Pickups",
                "Shows item names on pickup pings",
                true
            );

            ShowChestText = Config.Wrap(
                "ShowPingText",
                "Chests",
                "Shows item names and cost on chest pings",
                true
            );

            ShowShopText = Config.Wrap(
                "ShowPingText",
                "ShopTerminals",
                "Shows item names and cost on shop terminal pings",
                true
            );

            ShowDroneText = Config.Wrap(
                "ShowPingText",
                "Drones",
                "Shows drone type on broken drone pings",
                true
            );

            ShowShrineText = Config.Wrap(
                "ShowPingText",
                "Shrines",
                "Shows shrine type on shrine pings",
                true
            );

            On.RoR2.PingerController.SetCurrentPing += PingerController_SetCurrentPing;

            // If the scene unloads (e.g. we switch stages), clear the list of PingIndicators,
            // as all of them get inactive anyway in order to prevent memory leaks
            SceneManager.sceneUnloaded += (scene) =>
            {
                _pingIndicators.Clear();
            };
        }
        #endregion

        /**
         * <summary>
         *      Override for <see cref="PingerController.SetCurrentPing"/>
         * </summary>
         */
        private void PingerController_SetCurrentPing(On.RoR2.PingerController.orig_SetCurrentPing orig, 
            PingerController self, PingerController.PingInfo newPingInfo)
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
            GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/PingIndicator"));
            PingIndicator pingIndicator = go.GetComponent<PingIndicator>();

            pingIndicator.pingOwner = self.gameObject;
            pingIndicator.pingOrigin = newPingInfo.origin;
            pingIndicator.pingNormal = newPingInfo.normal;
            pingIndicator.pingTarget = newPingInfo.targetGameObject;

            pingIndicator.RebuildPing();

            SetPingIndicatorColor(ref pingIndicator);

            float fixedTimer = 0f;

            PingIndicator.PingType pingType = pingIndicator.GetObjectValue<PingIndicator.PingType>("pingType");

            switch(pingType)
            {
                case PingIndicator.PingType.Default:
                    fixedTimer = DefaultPingLifetime.Value;
                    break;
                case PingIndicator.PingType.Enemy:
                    fixedTimer = EnemyPingLifetime.Value;
                    break;
                case PingIndicator.PingType.Interactable:
                    fixedTimer = InteractiblePingLifetime.Value;
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

        private void SetPingIndicatorColor(ref PingIndicator pingIndicator)
        {
            SpriteRenderer sprRenderer;
            Color textColor = new Color(0,0,0,0);

            PingIndicator.PingType pingType = pingIndicator.GetObjectValue<PingIndicator.PingType>("pingType");

            switch(pingType)
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
            if (shopTerminal && ShowShopText.Value)
            {
                string text = textStart;
                PickupIndex pickupIndex = shopTerminal.CurrentPickupIndex();
                PickupDef pickup = PickupCatalog.GetPickupDef(pickupIndex);
                text += shopTerminal.pickupIndexIsHidden ? "?"
                    : $"{Language.GetString(pickup.nameToken)}";
                pingIndicator.pingText.text += $"{text} ({price})";
                return;
            }

            GenericPickupController pickupController = pingIndicator.pingTarget.GetComponent<GenericPickupController>();
            if (pickupController && ShowPickupText.Value)
            {
                PickupDef pickup = PickupCatalog.GetPickupDef(pickupController.pickupIndex);
                pingIndicator.pingText.text += $"{textStart}{Language.GetString(pickup.nameToken)}";
            }

            ChestBehavior chest = pingIndicator.pingTarget.GetComponent<ChestBehavior>();
            if (chest && ShowChestText.Value)
            {
                pingIndicator.pingText.text += $"{textStart}{Util.GetBestBodyName(pingIndicator.pingTarget)} ({price})";
                return;
            }

            string name = Language.GetString(pingIndicator.pingTarget.GetComponent<PurchaseInteraction>().displayNameToken);
            
            // Drones
            SummonMasterBehavior summonMaster = pingIndicator.pingTarget.GetComponent<SummonMasterBehavior>();
            if (summonMaster && ShowDroneText.Value)
            {
                pingIndicator.pingText.text += $"{textStart}{name} ({price})";
                return;
            }

            if (ShowShrineText.Value) pingIndicator.pingText.text += $"{textStart}{name}";
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
