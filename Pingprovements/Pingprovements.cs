using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.UI;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Pingprovements
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.pixeldesu.pingprovements", "Pingprovements", "1.0.0")]
    public class Pingprovements : BaseUnityPlugin
    {
        /**
         * <summary>
         *      Config variable for the ping lifetime
         * </summary>
         */
        public static ConfigWrapper<int> PingIndicatorLifetime { get; set; }

        /**
         * <summary>
         *      As <see cref="PingerController"/> only can hold one instance of <see cref="PingIndicator"/>, we need to
         *      add our own storage for them. This holds all <see cref="PingIndicator"/>s of the current stage
         * </summary>
         */
        private List<PingIndicator> pingIndicators = new List<PingIndicator>();

        public void Awake()
        {
            PingIndicatorLifetime = Config.Wrap<int>(
                "Main",
                "PingIndicatorLifetime",
                "Time in seconds how long a ping indicator for interactibles should be shown on the map",
                10000
            );

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
            if (newPingInfo.targetGameObject != null && pingIndicators.Any(indicator => indicator.pingTarget == newPingInfo.targetGameObject))
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

            // If the target is an interactible (pingType => 2), change the fixedTimer instance variable to the configured ping lifetime
            if (
                (int)pingIndicator
                    .GetType()
                    .GetField("pingType", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(pingIndicator) == (int) PingIndicator.PingType.Interactable
               )
            {
                pingIndicator.GetType().GetField("fixedTimer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(pingIndicator, (float) PingIndicatorLifetime.Value);
            }

            // We add the ping indicator to our own local list
            pingIndicators.Add(pingIndicator);

            if (self.hasAuthority)
            {
                self.CallCmdPing(self.currentPing);
            }
        }
    }
}
