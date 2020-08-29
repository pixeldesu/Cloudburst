using BepInEx.Configuration;

namespace CommuniKeys
{
    public class CommuniKeysConfig
    {
        public CommuniKeysConfig(ConfigFile config)
        {
            KeyTextMapping = config.Bind(
                "General",
                "KeyTextMapping",
                "1;Yes!|2;No!|3;Ready for Teleporter?|4;Ready for Next Stage?|5;Shrine of the Mountain?|6;Final Stage?|7;Check for Newt Altars!|8;Don't take all items!",
                "Mapping of key input strings and the message that should be posted, separated by semicolons (;), full mappings are separated with pipes (|)"
            );
        }
        
        public ConfigEntry<string> KeyTextMapping { get; set; }
    }
}