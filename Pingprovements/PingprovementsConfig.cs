using BepInEx.Configuration;

namespace Pingprovements
{
    public class PingprovementsConfig
    {
        public PingprovementsConfig(ConfigFile config)
        {
            DefaultPingLifetime = config.Wrap(
                "Durations",
                "DefaultPingLifetime",
                "Time in seconds how long a regular 'walk to' ping indicator should be shown on the map",
                6
            );

            EnemyPingLifetime = config.Wrap(
                "Durations",
                "EnemyPingLifetime",
                "Time in seconds how long a ping indicator for enemies should be shown on the map",
                8
            );

            InteractiblePingLifetime = config.Wrap(
                "Durations",
                "InteractiblePingLifetime",
                "Time in seconds how long a ping indicator for interactibles should be shown on the map",
                30
            );

            DefaultPingColorConfig = config.Wrap(
                "Colors",
                "DefaultPingColor",
                "Color of the default ping, in UnityEngine.Color R/G/B/A Float format",
                "0.525,0.961,0.486,1.000"
            );

            DefaultPingSpriteColorConfig = config.Wrap(
                "SpriteColors",
                "DefaultPingSpriteColor",
                "Color of the default ping sprite, in UnityEngine.Color R/G/B/A Float format",
                "0.527,0.962,0.486,1.000"
            );

            EnemyPingColorConfig = config.Wrap(
                "Colors",
                "EnemyPingColor",
                "Color of the enemy ping, in UnityEngine.Color R/G/B/A Float format",
                "0.820,0.122,0.122,1.000"
            );

            EnemyPingSpriteColorConfig = config.Wrap(
                "SpriteColors",
                "EnemyPingSpriteColor",
                "Color of the enemy ping sprite, in UnityEngine.Color R/G/B/A Float format",
                "0.821,0.120,0.120,1.000"
            );

            InteractiblePingColorConfig = config.Wrap(
                "Colors",
                "InteractiblePingColor",
                "Color of the interactible ping, in UnityEngine.Color R/G/B/A Float format",
                "0.886,0.871,0.173,1.000"
            );

            InteractiblePingSpriteColorConfig = config.Wrap(
                "SpriteColors",
                "InteractiblePingSpriteColor",
                "Color of the interactible ping sprite, in UnityEngine.Color R/G/B/A Float format",
                "0.887,0.870,0.172,1.000"
            );

            ShowPickupText = config.Wrap(
                "ShowPingText",
                "Pickups",
                "Shows item names on pickup pings",
                true
            );

            ShowChestText = config.Wrap(
                "ShowPingText",
                "Chests",
                "Shows item names and cost on chest pings",
                true
            );

            ShowShopText = config.Wrap(
                "ShowPingText",
                "ShopTerminals",
                "Shows item names and cost on shop terminal pings",
                true
            );

            ShowDroneText = config.Wrap(
                "ShowPingText",
                "Drones",
                "Shows drone type on broken drone pings",
                true
            );

            ShowShrineText = config.Wrap(
                "ShowPingText",
                "Shrines",
                "Shows shrine type on shrine pings",
                true
            );
            
            ShowEnemyText = config.Wrap(
                "ShowPingText",
                "Enemies",
                "Shows names on enemy pings",
                true
            );

            ShowPingDistance = config.Wrap(
                "ShowPingText",
                "Distance",
                "Show distance to ping in ping label",
                true
            );

            HideOffscreenPingText = config.Wrap(
                "ShowPingText",
                "HideOffscreenPingText",
                "Hide text of offscreen pings to prevent cluttering",
                true
            );

            ShowItemNotification = config.Wrap(
                "Notifications",
                "ShowItemNotification",
                "Show pickup-style notification with description on ping of an already discovered item",
                true
            );
        }
        
        #region Durations Configuration Options
        
        /// <summary>
        /// Configuration value for the default ping lifetime
        /// </summary>
        public ConfigWrapper<int> DefaultPingLifetime { get; set; }
        
        /// <summary>
        /// Configuration value for the enemy ping lifetime
        /// </summary>
        public ConfigWrapper<int> EnemyPingLifetime { get; set; }
        
        /// <summary>
        /// Configuration value for the interactible ping lifetime
        /// </summary>
        public ConfigWrapper<int> InteractiblePingLifetime { get; set; }
        
        #endregion

        #region Colors Configuration Options
        
        /// <summary>
        /// Configuration value for the default ping color
        /// </summary>
        public ConfigWrapper<string> DefaultPingColorConfig { get; set; }
        
        /// <summary>
        /// Configuration value for the enemy ping color
        /// </summary>
        public ConfigWrapper<string> EnemyPingColorConfig { get; set; }
        
        /// <summary>
        /// Configuration value for the interactible ping color
        /// </summary>
        public ConfigWrapper<string> InteractiblePingColorConfig { get; set; }
        
        #endregion
        
        #region SpriteColors Configuration Options
        
        /// <summary>
        /// Configuration value for the default ping sprite color
        /// </summary>
        public ConfigWrapper<string> DefaultPingSpriteColorConfig { get; set; }
        
        /// <summary>
        /// Configuration value for the enemy ping sprite color
        /// </summary>
        public ConfigWrapper<string> EnemyPingSpriteColorConfig { get; set; }
        
        /// <summary>
        /// Configuration value for the interactible ping sprite color
        /// </summary>
        public ConfigWrapper<string> InteractiblePingSpriteColorConfig { get; set; }
        
        #endregion

        #region ShowPingText Configuration Options
        
        /// <summary>
        /// Configuration value to enable showing shop text on pings
        /// </summary>
        public ConfigWrapper<bool> ShowShopText { get; set; }
        
        /// <summary>
        /// Configuration value to enable showing chest text on pings
        /// </summary>
        public ConfigWrapper<bool> ShowChestText { get; set; }

        /// <summary>
        /// Configuration value to enable showing pickup text on pings
        /// </summary>
        public ConfigWrapper<bool> ShowPickupText { get; set; }

        /// <summary>
        /// Configuration value to enable showing drone text on pings
        /// </summary>
        public ConfigWrapper<bool> ShowDroneText { get; set; }
        
        /// <summary>
        /// Configuration value to enable showing shrine text on pings
        /// </summary>
        public ConfigWrapper<bool> ShowShrineText { get; set; }
        
        /// <summary>
        /// Configuration value to enable showing enemy names on pings
        /// </summary>
        public ConfigWrapper<bool> ShowEnemyText { get; set; }
        
        /// <summary>
        /// Configuration value to enable showing the distance to a ping
        /// </summary>
        public ConfigWrapper<bool> ShowPingDistance { get; set; }
        
        /// <summary>
        /// Configuration value to hide the ping label if a ping is offscreen
        /// </summary>
        public ConfigWrapper<bool> HideOffscreenPingText { get; set; }
        
        #endregion
        
        #region Notification Configuration Options
        
        /// <summary>
        /// Configuration value to hide the ping label if a ping is offscreen
        /// </summary>
        public ConfigWrapper<bool> ShowItemNotification { get; set; }
        
        #endregion
    }
}