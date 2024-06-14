using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TwitchTTSUser.Base;
using TwitchTTSUser.Models;

namespace TwitchTTSUser.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ConfigData Config { get; private set; }
    public TwitchService Twitch { get; private set; }
    private TTSService TTS;

    [ObservableProperty]
    public IBrush connectionColor = Brushes.White;

    [ObservableProperty]
    public string selectedUser = string.Empty;

    public MainViewModel() 
    {
        Config = new ConfigData();
        Twitch = new TwitchService();
        TTS = new TTSService();

        Config = ConfigData.LoadConfigData();
        Twitch.MessageForwarder = HandleMessage;
        Twitch.NewSelectedUser = HandleNewSelectedUser;
    }

    private void HandleMessage(string IncomingMessage)
    {
        if (!string.IsNullOrEmpty(IncomingMessage))
        {
            TTS.SayMessage(IncomingMessage);
        }
    }

    private void HandleNewSelectedUser(string NewUsername)
    {
        SelectedUser = NewUsername;
        TTS.ChooseRandomVoiceSetting();
    }

    public void ConnectButton(object msg)
    {
        ConnectionColor = Brushes.White;
        Config.SaveConfigData();
        ReadOnlyCollection<object> Type = (ReadOnlyCollection<object>)msg;
        if (Twitch.ConnectToChannel((string)Type[0], (string)Type[1], (string)Type[2]))
        {
            ConnectionColor = Brushes.Green;
        }
        else
        {
            ConnectionColor = Brushes.Red;
        }
    }

    public bool CanConnectButton(object msg) => !Twitch.IsConnected;
}
