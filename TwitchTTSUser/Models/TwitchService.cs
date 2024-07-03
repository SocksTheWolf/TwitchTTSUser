using System;
using System.Collections.ObjectModel;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Client.Events;

namespace TwitchTTSUser.Models
{
    public class TwitchService
    {
        private TwitchClient client;
        private Random rng = new Random();
        private bool IsConnecting = false;
        private ConfigData Config;

        // This is used by the TTS system to send messages
        public Action<string>? OnMessageSent { private get; set; }

        // This alerts other UI Objects whenever a new selected user has been made
        public Action<string>? OnNewSelectedUser { private get; set; }

        // This alerts on the service connection state, including issues that need to be resolved.
        // arg is true if everything is fine, false otherwise
        public Action<bool>? OnConnectionStatus { private get; set; }

        // The main storage for the users that are selected!
        private string SelectedUserName
        {
            get { return selectedUserName; }
            set
            {
                selectedUserName = value;
                if (OnNewSelectedUser != null)
                    OnNewSelectedUser.Invoke(value);
            }
        }
        private string selectedUserName = string.Empty;

        // Current users that are signed up
        public ObservableCollection<string> SignedUpUsers { get; private set; } = new ObservableCollection<string>();
        private bool CanSignup = false;

        public TwitchService(ConfigData InConfig)
        {
            Config = InConfig;
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            WebSocketClient customClient = new WebSocketClient(clientOptions);

            client = new TwitchClient(customClient);
            client.AutoReListenOnException = true;
#pragma warning disable CS8622
            client.OnChatCommandReceived += OnCommandReceived;
            client.OnMessageReceived += OnMessageReceived;
            client.OnJoinedChannel += OnServiceJoined;
#pragma warning restore CS8622
        }

        public bool ConnectToChannel()
        {
            if (IsConnecting)
            {
                Console.WriteLine("Channel is already initialized!");
                if (OnConnectionStatus != null)
                    OnConnectionStatus.Invoke(false);
                return false;
            }

            IsConnecting = true;
            ConnectionCredentials creds = new ConnectionCredentials(Config.BotUserName, Config.OAuthToken);
            client.Initialize(creds, Config.ChannelName);
            bool ConnectionReturn = client.Connect();
            if (!ConnectionReturn)
            {
                Console.WriteLine("Could not connect to channel!");
                if (OnConnectionStatus != null)
                    OnConnectionStatus.Invoke(false);
                IsConnecting = false;
            }
            return ConnectionReturn;
        }

        private void OnServiceJoined(object unused, OnJoinedChannelArgs args)
        {
            client.SendMessage(args.Channel, "Twitch User Selector: Connected and ready for messages!");
            if (OnConnectionStatus != null)
                OnConnectionStatus.Invoke(true);
            ClearUser();
        }

        private void OnMessageReceived(object unused, OnMessageReceivedArgs args)
        {
            if (args.ChatMessage.DisplayName == SelectedUserName)
            {
                string MessageText = args.ChatMessage.Message;
                if (OnMessageSent != null)
                    OnMessageSent.Invoke(MessageText);
            }
        }

        private void OnCommandReceived(object unused, OnChatCommandReceivedArgs args)
        {
            string CommandValue = args.Command.CommandText.ToLower();

            // Check Admin Commands
            switch (CommandValue)
            {
                case "draw":
                case "pick":
                    if (args.Command.ChatMessage.IsBroadcaster)
                    {
                        PickUser();
                    }
                    break;
                case "open":
                    if (args.Command.ChatMessage.IsBroadcaster)
                    {
                        ClearUser();
                    }
                    break;
            }

            // Do a quick Exists check to see if the command exists in the array of command values.
            // If it does not, then this command was not for us.
            if (Config.IsValid && Array.Exists(Config.EntryCommands, element => element == CommandValue))
            {
                if (!CanSignup)
                    return;

                string SenderName = args.Command.ChatMessage.DisplayName;
                // Check to see if we can add the user (isn't already in the array)
                if (!SignedUpUsers.Contains(SenderName))
                {
                    // Add the user to the array.
                    SignedUpUsers.Add(SenderName);

                    // If we should send a message in chat, do so, by @ responding to said user.
                    if (Config.RespondToEntries)
                        client.SendMessage(args.Command.ChatMessage.Channel, $"@{SenderName} you have entered.");
                }
            }
        }

        private string GetChannelName() => Config.ChannelName.ToLower();

        public void ClearUser()
        {
            if (!client.IsConnected || CanSignup && Config.CloseSignupsOnDraw)
                return;

            CanSignup = true;
            SelectedUserName = string.Empty;
            SignedUpUsers.Clear();

            // Check to see if we have any entry commands (we should have a few defaults)
            if (Config.EntryCommands.Length >= 1)
                client.SendMessage(GetChannelName(), $"{Config.SignupsOpenText} Type !{Config.EntryCommands[0]}");
        }

        public void PickUser()
        {
            if (!client.IsConnected)
                return;

            if (SignedUpUsers.Count == 0)
                return;

            int ChooseIndex = 0;

            if (Config.ChooseUserRandomly)
                ChooseIndex = rng.Next(SignedUpUsers.Count);

            SelectedUserName = SignedUpUsers[ChooseIndex];
            if (Config.CloseSignupsOnDraw)
                CanSignup = false;

            SignedUpUsers.RemoveAt(ChooseIndex);
            client.SendMessage(GetChannelName(), $"@{SelectedUserName} {Config.SelectedUserText}");
        }
    }
}
