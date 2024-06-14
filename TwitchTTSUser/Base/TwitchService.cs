﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Client.Events;

namespace TwitchTTSUser.Base
{
    public class TwitchService
    {
        private TwitchClient client;
        private Random rng = new Random();
        private bool IsConnecting = false;

        // This is used by the TTS system to send messages
        public Action<string> MessageForwarder { private get; set; }

        // This alerts other UI Objects whenever a new selected user has been made
        public Action<string> NewSelectedUser { private get; set; }

        // The main storage for the users that are selected!
        private string SelectedUserName
        {
            get { return selectedUserName; }
            set
            {
                selectedUserName = value;
                NewSelectedUser.Invoke(value);
            }
        }
        private string selectedUserName = string.Empty;
        private List<string> SignedUpUsers = new List<string>();
        private bool CanSignup = false;

        public TwitchService()
        {
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

            // By default write all TTS stuff to the console until something overrides it.
            MessageForwarder = s => Console.WriteLine(s);
            NewSelectedUser = s => Console.WriteLine(s);
        }

        public bool IsConnected => client.IsConnected;

        public bool ConnectToChannel(string ChannelName, string UserName, string AccessToken)
        {
            if (IsConnecting)
            {
                Console.WriteLine("Channel is already initialized!");
                return false;
            }

            IsConnecting = true;
            ConnectionCredentials creds = new ConnectionCredentials(UserName, AccessToken);
            client.Initialize(creds, ChannelName);
            bool ConnectionReturn = client.Connect();
            if (!ConnectionReturn)
            {
                Console.WriteLine("Could not connect to channel!");
                IsConnecting = false;
            }
            return ConnectionReturn;
        }

        private void OnServiceJoined(object unused, OnJoinedChannelArgs args)
        {
            client.SendMessage(args.Channel, "Connected and ready for messages!");
            ClearUser();
        }

        private void OnMessageReceived(object unused, OnMessageReceivedArgs args)
        {
            if (args.ChatMessage.DisplayName == SelectedUserName)
            {
                string MessageText = args.ChatMessage.Message;

                // Write to file and TTS Service
                WriteFileData(false, MessageText);
                MessageForwarder.Invoke(MessageText);
            }
        }

        private void OnCommandReceived(object unused, OnChatCommandReceivedArgs args)
        {
            string SenderName = args.Command.ChatMessage.DisplayName;
            string ChannelName = args.Command.ChatMessage.Channel;
            switch (args.Command.CommandText)
            {
                case "draw":
                case "pick":
                    if (args.Command.ChatMessage.IsBroadcaster)
                    {
                        PickMayor();
                    }
                    break;
                case "enter":
                case "play":
                case "signup":
                    if (!CanSignup)
                        return;

                    if (!SignedUpUsers.Contains(SenderName))
                    {
                        SignedUpUsers.Add(SenderName);
                        client.SendMessage(ChannelName, $"@{SenderName} you have entered.");
                    }
                    break;
                case "open":
                    if (args.Command.ChatMessage.IsBroadcaster)
                    {
                        ClearUser();
                    }
                    break;
            }
        }

        private string GetChannelName()
        {
            if (!client.IsConnected)
                return string.Empty;

            return client.JoinedChannels.First().Channel;
        }

        private void WriteFileData(bool IsName, string data)
        {
            string FileName = (IsName) ? "username.txt" : "message.txt";
            using (StreamWriter FileWriter = File.CreateText(FileName))
            {
                FileWriter.WriteLineAsync(data);
            }
        }

        public void ClearUser(object? unused=null)
        {
            if (!client.IsConnected || CanSignup)
                return;

            CanSignup = true;
            SelectedUserName = string.Empty;
            SignedUpUsers.Clear();
            WriteFileData(true, string.Empty);
            WriteFileData(false, string.Empty);
            client.SendMessage(GetChannelName(), "Signups are now open! Type !signup to enter");
        }

        public void PickMayor(object? unused=null)
        {
            if (!client.IsConnected)
                return;

            if (SignedUpUsers.Count == 0)
                return;

            int RandomIndex = rng.Next(SignedUpUsers.Count);
            SelectedUserName = SignedUpUsers[RandomIndex];
            CanSignup = false;
            SignedUpUsers.RemoveAt(RandomIndex);
            WriteFileData(true, SelectedUserName);
            
            client.SendMessage(GetChannelName(), $"@{SelectedUserName} is now the new mayor!");
        }
    }
}
