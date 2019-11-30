using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pingprovements
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.pixeldesu.pingprovements", "Pingprovements", "1.2.0")]
    public class Pingprovements : BaseUnityPlugin
    {
        #region Private Fields

        /// <summary>
        /// Configuration instance
        /// </summary>
        private static PingprovementsConfig _config;

        #endregion

        #region Configuration and Startup

        public Pingprovements()
        {
            _config = new PingprovementsConfig(Config);
        }

        public void Awake()
        {
            PingerController pingerController = new PingerController(this);

            On.RoR2.PingerController.SetCurrentPing += pingerController.SetCurrentPing;

            SceneManager.sceneUnloaded += pingerController.OnSceneUnloaded;
        }

        #endregion

        public PingprovementsConfig GetConfig() => _config;
    }
}