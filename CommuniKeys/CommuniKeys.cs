using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BepInEx;
using RoR2;
using UnityEngine;
using Console = RoR2.Console;

namespace CommuniKeys
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.pixeldesu.communikeys", "CommuniKeys", "1.0.0")]
    public class CommuniKeys : BaseUnityPlugin
    {
        private static CommuniKeysConfig _config;

        private static List<KeyValuePair<string, string>> _mapping;

        public CommuniKeys()
        {
            _config = new CommuniKeysConfig(Config);
        }

        public void Awake()
        {
            _mapping = BuildMapping(_config.KeyTextMapping.Value).ToList();
        }

        public void Update()
        {
            foreach (KeyValuePair<string,string> mapping in _mapping)
            {
                try
                {
                    if (Input.GetKeyDown(mapping.Key))
                    {
                        SubmitChat(mapping.Value);
                    }
                }
                catch (ArgumentException e)
                {
                    // If the current mapping causes an ArgumentException (in 99% of cases a invalid key mapping)
                    // we'll just delete the key/message mapping to avoid further errors per frame
                    _mapping.Remove(mapping);
                }
            }
        }

        /// <summary>
        /// Method to send a message to chat. The escaping exactly mirrors the chatbox behavior from Risk of Rain 2
        /// </summary>
        /// <param name="message">The message to be sent</param>
        private void SubmitChat(string message)
        {
            ReadOnlyCollection<NetworkUser> readOnlyLocalPlayersList = NetworkUser.readOnlyLocalPlayersList;
            if (readOnlyLocalPlayersList.Count > 0)
            {
                message = message.Replace("\\", "\\\\");
                message = message.Replace("\"", "\\\"");
                Console.instance.SubmitCmd(readOnlyLocalPlayersList[0], "say \"" + message + "\"", false);
            }
        }

        /// <summary>
        /// Turns the config string containing the key-message mapping into a dictionary
        /// </summary>
        /// <param name="configString">The configuration string containing the mapping</param>
        /// <returns></returns>
        private Dictionary<string, string> BuildMapping(string configString)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string[] comMessages = configString.Split('|');
            
            foreach (string comMessage in comMessages)
            {
                string[] keyMessage = comMessage.Split(';');

                if (keyMessage.Length == 2)
                {
                    dict.Add(keyMessage[0], keyMessage[1]);
                }
            }

            return dict;
        }
    }
}