using System.Collections.Generic;
using UnityEngine;

namespace Pingprovements
{
    public class PingPainter
    {
        /// <summary>
        /// Color instances used for the <see cref="PingIndicator"/>s
        /// </summary>
        private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>();
        
        public PingPainter(PingprovementsConfig config)
        {
            _colors.Add("DefaultPingColor", config.DefaultPingColorConfig.Value.ToColor());
            _colors.Add("DefaultPingSpriteColor", config.DefaultPingSpriteColorConfig.Value.ToColor());
            _colors.Add("EnemyPingColor", config.EnemyPingColorConfig.Value.ToColor());
            _colors.Add("EnemyPingSpriteColor", config.EnemyPingSpriteColorConfig.Value.ToColor());
            _colors.Add("InteractablePingColor", config.InteractablePingColorConfig.Value.ToColor());
            _colors.Add("InteractablePingSpriteColor", config.InteractablePingSpriteColorConfig.Value.ToColor());
        }
        
        /// <summary>
        /// Sets the ping text and sprite color for a given <see cref="PingIndicator"/>
        /// </summary>
        /// <param name="pingIndicator">Target <see cref="PingIndicator"/></param>
        public void SetPingIndicatorColor(RoR2.UI.PingIndicator pingIndicator)
        {
            SpriteRenderer sprRenderer;
            Color textColor = new Color(0, 0, 0, 0);

            RoR2.UI.PingIndicator.PingType pingType =
                pingIndicator.GetObjectValue<RoR2.UI.PingIndicator.PingType>("pingType");

            switch (pingType)
            {
                case RoR2.UI.PingIndicator.PingType.Default:
                    textColor = _colors["DefaultPingColor"];
                    sprRenderer = pingIndicator.defaultPingGameObjects[0].GetComponent<SpriteRenderer>();
                    sprRenderer.color = _colors["DefaultPingSpriteColor"];
                    break;
                case RoR2.UI.PingIndicator.PingType.Enemy:
                    textColor = _colors["EnemyPingColor"];
                    sprRenderer = pingIndicator.enemyPingGameObjects[0].GetComponent<SpriteRenderer>();
                    sprRenderer.color = _colors["EnemyPingSpriteColor"];
                    break;
                case RoR2.UI.PingIndicator.PingType.Interactable:
                    textColor = _colors["InteractablePingColor"];
                    sprRenderer = pingIndicator.interactablePingGameObjects[0].GetComponent<SpriteRenderer>();
                    sprRenderer.color = _colors["InteractablePingSpriteColor"];
                    break;
            }

            pingIndicator.pingText.color = textColor;
        }
    }
}