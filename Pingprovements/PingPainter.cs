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
        /// <param name="pingType">Type of the ping</param>
        public void SetPingIndicatorColor(RoR2.UI.PingIndicator pingIndicator, RoR2.UI.PingIndicator.PingType pingType)
        {
            SpriteRenderer sprRenderer = new SpriteRenderer();
            Color textColor = new Color(0, 0, 0, 0);
            Color spriteColor = new Color(0, 0, 0, 0);

            switch (pingType)
            {
                case RoR2.UI.PingIndicator.PingType.Default:
                    sprRenderer = pingIndicator.defaultPingGameObjects[0].GetComponent<SpriteRenderer>();
                    textColor = _colors["DefaultPingColor"];
                    spriteColor = _colors["DefaultPingSpriteColor"];
                    break;
                case RoR2.UI.PingIndicator.PingType.Enemy:
                    sprRenderer = pingIndicator.enemyPingGameObjects[0].GetComponent<SpriteRenderer>();
                    textColor = _colors["EnemyPingColor"];
                    spriteColor = _colors["EnemyPingSpriteColor"];
                    break;
                case RoR2.UI.PingIndicator.PingType.Interactable:
                    sprRenderer = pingIndicator.interactablePingGameObjects[0].GetComponent<SpriteRenderer>();
                    textColor = _colors["InteractablePingColor"];
                    spriteColor = _colors["InteractablePingSpriteColor"];
                    break;
            }

            pingIndicator.pingText.color = textColor;
            sprRenderer.color = spriteColor;
        }
    }
}