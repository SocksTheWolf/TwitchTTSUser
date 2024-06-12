using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Client.Events;

namespace TwitchTTSUser.Base
{
    public class TwitchService
    {
        // TODO: Some sort of delegate or callback for when messages are handled to send to TTS
        private TwitchClient client;

        public TwitchService()
        {
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);

            
            client.OnChatCommandReceived += OnCommandReceived;
            client.OnMessageReceived += OnMessageReceived;
            client.OnJoinedChannel += OnServiceJoined;
        }

        public bool ConnectToChannel(string ChannelName, string UserName, string AccessToken)
        {
            if (client.IsInitialized)
            {
                Console.WriteLine("Channel is already initialized!");
                return false;
            }

            ConnectionCredentials creds = new ConnectionCredentials(UserName, AccessToken);
            client.Initialize(creds, ChannelName);
            return client.Connect();
        }

        private void OnServiceJoined(object unused, OnJoinedChannelArgs args)
        {
            client.SendMessage(args.Channel, "Connected and ready for messages!");
        }

        private void OnMessageReceived(object unused, OnMessageReceivedArgs args)
        {
            // TODO: All of the logic here
        }

        private void OnCommandReceived(object unused, OnChatCommandReceivedArgs args)
        {
            // TODO: All of the logic here
        }
    }
}
