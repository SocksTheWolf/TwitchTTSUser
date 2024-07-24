using System;
using System.IO;
using Newtonsoft.Json;

namespace TwitchTTSUser.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ConfigData
    {
        // Internals
        private int Internal_VoiceVolume = 100;
        private int Internal_VoiceRateBounds = 3;
        public bool IsValid { get; private set; } = true;

        // Statics
        public static string FileName = "config.json";

        /*** Twitch Connection Information ***/
        [JsonProperty]
        public string ChannelName { get; set; } = string.Empty;

        [JsonProperty]
        public string BotUserName { get; set; } = string.Empty;

        [JsonProperty]
        public string OAuthToken { get; set; } = string.Empty;

        /*** Behavior Settings ***/
        // Message to show in chat when an user has been chosen!
        [JsonProperty]
        public string SelectedUserText { get; set; } = "has been choosen!";

        // Message to show in chat when signups are open!
        [JsonProperty]
        public string SignupsOpenText { get; set; } = "Signups are now open!";

        // Prefix of message to say in TTS when someone is chosen
        [JsonProperty]
        public string SelectedWelcomePrefix { get; set; } = "New User:";

        // Read out the chosen username over TTS when selected
        [JsonProperty]
        public bool ReadSelectedUserName { get; set; } = true;

        // Cult of the Lamb style of drawing, where a bunch of entries are gathered in a window and then processed through.
        [JsonProperty]
        public bool CloseSignupsOnDraw { get; set; } = false;

        // If true, will randomly choose someone in the queue, rather than go sequentially.
        [JsonProperty]
        public bool ChooseUserRandomly { get; set; } = true;

        // When the timer is up, automatically choose the next person in the queue.
        [JsonProperty]
        public bool AutoChooseNextPerson { get; set; } = false;

        // TTS and File Updates will only run if the clock currently has time and is running
        [JsonProperty]
        public bool InteractOnlyIfClockRunning { get; set; } = false;

        // The maximum amount of time in seconds for someone to play
        [JsonProperty]
        public int MaxSelectedTime { get; set; } = 300;

        // Commands that the bot should respond to.
        // These will always be converted to lower case and have their ! symbols removed if they exist.
        [JsonProperty]
        public string[] EntryCommands { get; set; } = { "!enTer", "!join", "!play", "!signup" };

        // TTS Voice volume (from 0-100)
        [JsonProperty]
        public int VoiceVolume { 
            get { return Internal_VoiceVolume; }
            set { Internal_VoiceVolume = Math.Clamp(value, 0, 100); }
        }

        // TTS Read Rate Bounds (will go from +- of this number, max 10)
        [JsonProperty]
        public int VoiceRateBounds {
            get { return Internal_VoiceRateBounds; }
            set { Internal_VoiceRateBounds = Math.Clamp(value, 0, 10); }
        }

        // Max character limit for messages. The message will be truncated to this size if it's larger than it.
        [JsonProperty]
        public int MaxMessageSize { get; set; } = 400;

        // If the bot should respond in chat whenever someone enters.
        [JsonProperty]
        public bool RespondToEntries { get; set; } = true;

        /*** Config Loading/Saving ***/
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
                        // Clean up EntryCommands
                        string CleanUpCommands(string Input)
                        {
                            // Remove the ! at the start of the command
                            if (Input.StartsWith('!'))
                                Input = Input.Substring(1);

                            // Make sure everything is lower cased.
                            return Input.ToLower();
                        }
                        outputConfig.EntryCommands = Array.ConvertAll(outputConfig.EntryCommands, new Converter<string, string>(CleanUpCommands));

                        configData = outputConfig;
                        Console.WriteLine("Settings loaded");
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to load settings");
                    configData.IsValid = false;
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
            }
        }
    }
}
