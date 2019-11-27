using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.UI;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

namespace Pingprovements
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.pixeldesu.pingprovements", "Pingprovements", "1.1.0")]
    public class Pingprovements : BaseUnityPlugin
    {
        // Config variables for ping lifetimes
        public static ConfigWrapper<int> DefaultPingLifetime { get; set; }
        public static ConfigWrapper<int> EnemyPingLifetime { get; set; }
        public static ConfigWrapper<int> InteractiblePingLifetime { get; set; }


        // Config variables for ping text colors
        public static ConfigWrapper<string> DefaultPingColorConfig { get; set; }
        public static ConfigWrapper<string> EnemyPingColorConfig { get; set; }
        public static ConfigWrapper<string> InteractiblePingColorConfig { get; set; }


        // Config variables for ping sprite colors
        public static ConfigWrapper<string> DefaultPingSpriteColorConfig { get; set; }
        public static ConfigWrapper<string> EnemyPingSpriteColorConfig { get; set; }
        public static ConfigWrapper<string> InteractiblePingSpriteColorConfig { get; set; }


        // Color instances
        private Color DefaultPingColor;
        private Color DefaultPingSpriteColor;
        private Color EnemyPingColor;
        private Color EnemyPingSpriteColor;
        private Color InteractiblePingColor;
        private Color InteractiblePingSpriteColor;


        /**
         * <summary>
         *      As <see cref="PingerController"/> only can hold one instance of <see cref="PingIndicator"/>, we need to
         *      add our own storage for them. This holds all <see cref="PingIndicator"/>s of the current stage
         * </summary>
         */
        private List<PingIndicator> pingIndicators = new List<PingIndicator>();

        public void Awake()
        {
            DefaultPingLifetime = Config.Wrap<int>(
                "Durations",
                "DefaultPingLifetime",
                "Time in seconds how long a regular 'walk to' ping indicator should be shown on the map",
                6
            );

            EnemyPingLifetime = Config.Wrap<int>(
                "Durations",
                "EnemyPingLifetime",
                "Time in seconds how long a ping indicator for enemies should be shown on the map",
                8
            );

            InteractiblePingLifetime = Config.Wrap<int>(
                "Durations",
                "InteractiblePingLifetime",
                "Time in seconds how long a ping indicator for interactibles should be shown on the map",
                30
            );

            DefaultPingColorConfig = Config.Wrap<string>(
                "Colors",
                "DefaultPingColor",
                "Color of the default ping, in UnityEngine.Color R/G/B/A Float format",
                "0.525,0.961,0.486,1.000"
            );

            DefaultPingSpriteColorConfig = Config.Wrap<string>(
                "SpriteColors",
                "DefaultPingSpriteColor",
                "Color of the default ping sprite, in UnityEngine.Color R/G/B/A Float format",
                "0.527,0.962,0.486,1.000"
            );

            DefaultPingColor = ConvertStringToColor(DefaultPingColorConfig.Value);
            DefaultPingSpriteColor = ConvertStringToColor(DefaultPingSpriteColorConfig.Value);

            EnemyPingColorConfig = Config.Wrap<string>(
                "Colors",
                "EnemyPingColor",
                "Color of the enemy ping, in UnityEngine.Color R/G/B/A Float format",
                "0.820,0.122,0.122,1.000"
            );

            EnemyPingSpriteColorConfig = Config.Wrap<string>(
                "SpriteColors",
                "EnemyPingSpriteColor",
                "Color of the enemy ping sprite, in UnityEngine.Color R/G/B/A Float format",
                "0.821,0.120,0.120,1.000"
            );

            EnemyPingColor = ConvertStringToColor(EnemyPingColorConfig.Value);
            EnemyPingSpriteColor = ConvertStringToColor(EnemyPingSpriteColorConfig.Value);

            InteractiblePingColorConfig = Config.Wrap<string>(
                "Colors",
                "InteractiblePingColor",
                "Color of the interactible ping, in UnityEngine.Color R/G/B/A Float format",
                "0.886,0.871,0.173,1.000"
            );

            InteractiblePingSpriteColorConfig = Config.Wrap<string>(
                "SpriteColors",
                "InteractiblePingSpriteColor",
                "Color of the interactible ping sprite, in UnityEngine.Color R/G/B/A Float format",
                "0.887,0.870,0.172,1.000"
            );

            InteractiblePingColor = ConvertStringToColor(InteractiblePingColorConfig.Value);
            InteractiblePingSpriteColor = ConvertStringToColor(InteractiblePingSpriteColorConfig.Value);

            On.RoR2.PingerController.SetCurrentPing += PingerController_SetCurrentPing;

            // If the scene unloads (e.g. we switch stages), clear the list of PingIndicators, as all of them get inactive anyway
            // to prevent memory leaks
            SceneManager.sceneUnloaded += (scene) =>
            {
                pingIndicators.Clear();
            };
        }

        /**
         * <summary>
         *      Override for <see cref="PingerController.SetCurrentPing"/>
         * </summary>
         */
        private void PingerController_SetCurrentPing(On.RoR2.PingerController.orig_SetCurrentPing orig, PingerController self, PingerController.PingInfo newPingInfo)
        {
            // If the targeted game object already has a ping, don't do anything
            // This is here to avoid stacking of different player pings on interactibles
            if (newPingInfo.targetGameObject != null && pingIndicators.Any(indicator => indicator && indicator.pingTarget == newPingInfo.targetGameObject))
                return;

            self.NetworkcurrentPing = newPingInfo;

            // Here we create an instance of PingIndicator
            // since we're not jumping into PingerController.RebuildPing() to create one
            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/PingIndicator"));
            PingIndicator pingIndicator = gameObject.GetComponent<PingIndicator>();

            pingIndicator.pingOwner = self.gameObject;
            pingIndicator.pingOrigin = newPingInfo.origin;
            pingIndicator.pingNormal = newPingInfo.normal;
            pingIndicator.pingTarget = newPingInfo.targetGameObject;

            pingIndicator.RebuildPing();

            SetPingIndicatorColor(ref pingIndicator);

            float fixedTimer = 0f;

            PingIndicator.PingType pingType = (PingIndicator.PingType) pingIndicator
                    .GetType()
                    .GetField("pingType", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(pingIndicator);

            switch(pingType)
            {
                case PingIndicator.PingType.Default:
                    fixedTimer = (float) DefaultPingLifetime.Value;
                    break;
                case PingIndicator.PingType.Enemy:
                    fixedTimer = (float) EnemyPingLifetime.Value;
                    break;
                case PingIndicator.PingType.Interactable:
                    fixedTimer = (float) InteractiblePingLifetime.Value;
                    break;
            }

            pingIndicator
                .GetType()
                .GetField("fixedTimer", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(pingIndicator, fixedTimer);

            // We add the ping indicator to our own local list
            pingIndicators.Add(pingIndicator);

            if (self.hasAuthority)
            {
                self.CallCmdPing(self.currentPing);
            }
        }

        private void SetPingIndicatorColor(ref PingIndicator pingIndicator)
        {
            SpriteRenderer sprRenderer;
            Color textColor = new Color(0,0,0,0);

            PingIndicator.PingType pingType = (PingIndicator.PingType) pingIndicator
                    .GetType()
                    .GetField("pingType", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(pingIndicator);

            switch(pingType)
            {
                case PingIndicator.PingType.Default:
                    textColor = DefaultPingColor;
                    sprRenderer = pingIndicator.defaultPingGameObjects[0].GetComponent<SpriteRenderer>();
                    sprRenderer.color = DefaultPingSpriteColor;
                    break;
                case PingIndicator.PingType.Enemy:
                    textColor = EnemyPingColor;
                    sprRenderer = pingIndicator.enemyPingGameObjects[0].GetComponent<SpriteRenderer>();
                    sprRenderer.color = EnemyPingSpriteColor;
                    break;
                case PingIndicator.PingType.Interactable:
                    textColor = InteractiblePingColor;
                    sprRenderer = pingIndicator.interactablePingGameObjects[0].GetComponent<SpriteRenderer>();
                    sprRenderer.color = InteractiblePingSpriteColor;
                    break;
            }

            pingIndicator.pingText.color = textColor;
        }

        private Color ConvertStringToColor(string colorString)
        {
            float[] colorValues = Array.ConvertAll(colorString.Split(','), float.Parse);

            return new Color(colorValues[0], colorValues[1], colorValues[2], colorValues[3]);
        }
    }
}
