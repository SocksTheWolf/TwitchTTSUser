using System;
using System.IO;
using Newtonsoft.Json;

namespace TwitchTTSUser.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ConfigData
    {
        public static string FileName = "config.json";

        [JsonProperty]
        public string ChannelName { get; set; } = string.Empty;

        [JsonProperty]
        public string BotUserName { get; set; } = string.Empty;

        [JsonProperty]
        public string OAuthToken { get; set; } = string.Empty;

        public static ConfigData LoadConfigData()
        {
            ConfigData configData = new ConfigData();
            if (!File.Exists(FileName))
            {
                configData.SaveConfigData();
                return configData;
            }

            string json = File.ReadAllText(FileName);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var outputConfig = JsonConvert.DeserializeObject<ConfigData>(json);
                    if(outputConfig != null)
                    {
                        configData = outputConfig;
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to load settings");
                }
            }
            
            return configData;
        }

        public void SaveConfigData()
        {
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (StreamWriter FileWriter = File.CreateText(FileName))
            {
                FileWriter.WriteLine(jsonString);
                Console.WriteLine("Wrote file successfully!");
            }
        }
    }
}
