using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
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
        public static ConfigWrapper<int> PingIndicatorLifetime { get; set; }

        private List<RoR2.UI.PingIndicator> pingIndicators = new List<RoR2.UI.PingIndicator>();

        public void Awake()
        {
            PingIndicatorLifetime = Config.Wrap<int>(
                "Main",
                "PingIndicatorLifetime",
                "Time in seconds how long a ping indicator for interactibles should be shown on the map",
                10000
            );

            On.RoR2.PingerController.SetCurrentPing += PingerController_SetCurrentPing;

            SceneManager.sceneUnloaded += (scene) =>
            {
                pingIndicators.Clear();
            };
        }

        private void PingerController_SetCurrentPing(On.RoR2.PingerController.orig_SetCurrentPing orig, PingerController self, PingerController.PingInfo newPingInfo)
        {
            if (newPingInfo.targetGameObject != null && pingIndicators.Any(indicator => indicator.pingTarget == newPingInfo.targetGameObject))
                return;

            self.NetworkcurrentPing = newPingInfo;

            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/PingIndicator"));
            RoR2.UI.PingIndicator pingIndicator = gameObject.GetComponent<RoR2.UI.PingIndicator>();

            pingIndicator.pingOwner = self.gameObject;
            pingIndicator.pingOrigin = newPingInfo.origin;
            pingIndicator.pingNormal = newPingInfo.normal;
            pingIndicator.pingTarget = newPingInfo.targetGameObject;
            pingIndicator.RebuildPing();

            if (
                (int)pingIndicator
                    .GetType()
                    .GetField("pingType", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(pingIndicator) == 2
               )
            {
                pingIndicator.GetType().GetField("fixedTimer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(pingIndicator, (float) PingIndicatorLifetime.Value);
            }

            pingIndicators.Add(pingIndicator);

            if (self.hasAuthority)
            {
                self.CallCmdPing(self.currentPing);
            }
        }
    }
}
