using BepInEx.Configuration;

namespace Pingprovements
{
    class PingprovementsConfig
    {
        public PingprovementsConfig(ConfigFile Config)
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
        }

        // Config variables for ping lifetimes
        public ConfigWrapper<int> DefaultPingLifetime { get; set; }
        public ConfigWrapper<int> EnemyPingLifetime { get; set; }
        public ConfigWrapper<int> InteractiblePingLifetime { get; set; }


        // Config variables for ping text colors
        public ConfigWrapper<string> DefaultPingColorConfig { get; set; }
        public ConfigWrapper<string> EnemyPingColorConfig { get; set; }
        public ConfigWrapper<string> InteractiblePingColorConfig { get; set; }


        // Config variables for ping sprite colors
        public ConfigWrapper<string> DefaultPingSpriteColorConfig { get; set; }
        public ConfigWrapper<string> EnemyPingSpriteColorConfig { get; set; }
        public ConfigWrapper<string> InteractiblePingSpriteColorConfig { get; set; }

        // Config variables for interactible additional text
        public ConfigWrapper<bool> ShowShopText { get; set; }
        public ConfigWrapper<bool> ShowChestText { get; set; }
        public ConfigWrapper<bool> ShowPickupText { get; set; }
        public ConfigWrapper<bool> ShowDroneText { get; set; }
        public ConfigWrapper<bool> ShowShrineText { get; set; }
    }
}