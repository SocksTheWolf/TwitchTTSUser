using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Client.Events;
using TwitchTTSUser.Models;

namespace TwitchTTSUser.Base
{
    public class TwitchService
    {
        private TwitchClient client;
        private Random rng = new Random();
        private bool IsConnecting = false;
        private ConfigData Config;

        // This is used by the TTS system to send messages
        public Action<string> OnMessageSent { private get; set; }

        // This alerts other UI Objects whenever a new selected user has been made
        public Action<string> OnNewSelectedUser { private get; set; }

        // This alerts on the service connection state, including issues that need to be resolved.
        // arg is true if everything is fine, false otherwise
        public Action<bool> OnConnectionStatus { private get; set; }

        // The main storage for the users that are selected!
        private string SelectedUserName
        {
            get { return selectedUserName; }
            set
            {
                selectedUserName = value;
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

            // Set up basic redirects to the console
            OnNewSelectedUser = OnMessageSent = s => Console.WriteLine(s);
            OnConnectionStatus = s => Console.WriteLine(s);
            ClearFileData();
        }

        public bool ConnectToChannel(string ChannelName, string UserName, string AccessToken)
        {
            if (IsConnecting)
            {
                Console.WriteLine("Channel is already initialized!");
                OnConnectionStatus.Invoke(false);
                return false;
            }

            IsConnecting = true;
            ConnectionCredentials creds = new ConnectionCredentials(UserName, AccessToken);
            client.Initialize(creds, ChannelName);
            bool ConnectionReturn = client.Connect();
            if (!ConnectionReturn)
            {
                Console.WriteLine("Could not connect to channel!");
                OnConnectionStatus.Invoke(false);
                IsConnecting = false;
            }
            return ConnectionReturn;
        }

        private void OnServiceJoined(object unused, OnJoinedChannelArgs args)
        {
            client.SendMessage(args.Channel, "Twitch User Selector: Connected and ready for messages!");
            OnConnectionStatus.Invoke(true);
            ClearUser();
        }

        private void OnMessageReceived(object unused, OnMessageReceivedArgs args)
        {
            if (args.ChatMessage.DisplayName == SelectedUserName)
            {
                string MessageText = args.ChatMessage.Message;

                // Write to file and TTS Service
                WriteFileData(false, MessageText);
                OnMessageSent.Invoke(MessageText);
            }
        }

        private void OnCommandReceived(object unused, OnChatCommandReceivedArgs args)
        {
            string SenderName = args.Command.ChatMessage.DisplayName;
            string ChannelName = args.Command.ChatMessage.Channel;
            switch (args.Command.CommandText.ToLower())
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
                case "enter":
                case "play":
                case "signup":
                case "join":
                    if (!CanSignup)
                        return;

                    if (!SignedUpUsers.Contains(SenderName))
                    {
                        SignedUpUsers.Add(SenderName);
                        if (Config.RespondToEntries)
                            client.SendMessage(ChannelName, $"@{SenderName} you have entered.");
                    }
                    break;
            }
        }

        private string GetChannelName() => Config.ChannelName.ToLower();

        private void WriteFileData(bool IsName, string data)
        {
            string FileName = (IsName) ? "username.txt" : "message.txt";
            using (StreamWriter FileWriter = File.CreateText(FileName))
            {
                FileWriter.WriteLineAsync(data);
            }
        }

        private void ClearFileData()
        {
            WriteFileData(true, string.Empty);
            WriteFileData(false, string.Empty);
        }

        public void ClearUser(object? unused = null)
        {
            if (!client.IsConnected || (CanSignup && Config.CloseSignupsOnDraw))
                return;

            CanSignup = true;
            SelectedUserName = string.Empty;
            SignedUpUsers.Clear();
            ClearFileData();
            client.SendMessage(GetChannelName(), $"{Config.SignupsOpenText} Type !signup");
        }

        public void PickUser(object? unused = null)
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
            WriteFileData(true, SelectedUserName);
            WriteFileData(false, string.Empty);

            client.SendMessage(GetChannelName(), $"@{SelectedUserName} {Config.SelectedUserText}");
        }
    }
}
