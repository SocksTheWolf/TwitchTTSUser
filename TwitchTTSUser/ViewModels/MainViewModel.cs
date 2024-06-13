using System.Collections.ObjectModel;
using TwitchTTSUser.Base;
using TwitchTTSUser.Models;

namespace TwitchTTSUser.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ConfigData Config { get; set; } = new();
    public TTSService TTS => new();
    public TwitchService Twitch { get; set; } = new();

    public MainViewModel() 
    {
        Config = ConfigData.LoadConfigData();
        Twitch.MessageForwarder = s => TTS.SayMessage(s);
    }

    public void ConnectButton(object msg)
    {
        Config.SaveConfigData();
        ReadOnlyCollection<object> Type = (ReadOnlyCollection<object>)msg;
        Twitch.ConnectToChannel((string)Type[0], (string)Type[1], (string)Type[2]);
    }

    public bool CanConnectButton(object msg) => !Twitch.IsConnected;
}
