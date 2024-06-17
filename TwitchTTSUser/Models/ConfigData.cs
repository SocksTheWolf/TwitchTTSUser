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

        [JsonProperty]
        public string SelectedUserText { get; set; } = "has been choosen!";

        [JsonProperty]
        public string SignupsOpenText { get; set; } = "Signups are now open!";

        [JsonProperty]
        public bool CloseSignupsOnDraw { get; set; } = false;

        [JsonProperty]
        public bool ChooseUserRandomly { get; set; } = true;

        [JsonProperty]
        public int MaxSelectedTime { get; set; } = 300;

        [JsonProperty]
        public int VoiceVolume { get; set; } = 100;

        [JsonProperty]
        public int VoiceRate { get; set; } = 3;

        [JsonProperty]
        public bool RespondToEntries { get; set; } = true;

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
                    if (outputConfig != null)
                    {
                        configData = outputConfig;
                        Console.WriteLine("Settings loaded");
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
