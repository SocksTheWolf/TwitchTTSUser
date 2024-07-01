using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Timers;
using TwitchTTSUser.Base;
using TwitchTTSUser.Models;

namespace TwitchTTSUser.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ConfigData Config { get; private set; }
    public TwitchService Twitch { get; private set; }
    private TTSService TTS;
    private SFXService SFX;

    private Timer CountdownClock;
    private int CurrentTime = 0;

    [ObservableProperty]
    public IBrush connectionColor = Brushes.White;

    [ObservableProperty]
    public string selectedUser = string.Empty;

    [ObservableProperty]
    public string userTime = string.Empty;

    [ObservableProperty]
    public string timerButtonText = "Waiting...";

    // Avalonia Can prefix doesn't seem to reliably update Enabled on buttons, I'm not sure why
    // so instead bind IsEnabled to these booleans
    [ObservableProperty]
    public bool userSelected = false;

    [ObservableProperty]
    public bool isConnected = false; // If the user is connected to twitch (technically kind of a repeat of connectionColor)

    public MainViewModel() 
    {
        Config = ConfigData.LoadConfigData();

        SFX = new SFXService();
        Twitch = new TwitchService(Config);
        TTS = new TTSService(Config.VoiceVolume, Config.VoiceRate);

        // Shows a countdown clock on screen for each person's turn
        CountdownClock = new Timer();
        CountdownClock.Elapsed += new ElapsedEventHandler(UpdateClock);
        CountdownClock.Interval = 1000;

        Twitch.OnMessageSent = HandleMessage;
        Twitch.OnNewSelectedUser = HandleNewSelectedUser;
        Twitch.OnConnectionStatus = HandleConnectionStatus;
    }

    private void UpdateClock(object? sender, ElapsedEventArgs args)
    {
        // Updates the clock data.
        --CurrentTime;
        UserTime = TimeSpan.FromSeconds(CurrentTime).ToString(@"mm\:ss");
        if (CurrentTime <= 0)
        {
            // Time is up!
            SFX.PlayAudio();
            CountdownClock.Stop();

            if (Config.AutoChooseNextPerson)
                Twitch.PickUser(); // If successful, will callback to HandleNewSelectedUser
        }
    }

    private void HandleConnectionStatus(bool Connected)
    {
        ConnectionColor = (Connected) ? Brushes.Green : Brushes.Red;
        IsConnected = Connected;
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
        UserTime = string.Empty;

        if (!string.IsNullOrEmpty(NewUsername))
        {
            UserSelected = true;
            TTS.ChooseRandomVoiceSetting();
            // Read the name of the new user that was selected
            if (Config.ReadSelectedUserName)
                TTS.SayMessage($"{Config.SelectedWelcomePrefix}: {NewUsername}");

            CurrentTime = Config.MaxSelectedTime;
            CountdownClock.Start();
        }
        else
        {
            UserSelected = false;
            CountdownClock.Stop();
        }
        UpdatePauseButtonText();
    }

    /*** BUTTONS ***/
    public void ConnectButton(object msg)
    {
        ConnectionColor = Brushes.White;
        Config.SaveConfigData();
        ReadOnlyCollection<object> Type = (ReadOnlyCollection<object>)msg;
        if (Twitch.ConnectToChannel((string)Type[0], (string)Type[1], (string)Type[2]))
        {
            // Set our state to Orange. It will tell us what further thing to do.
            ConnectionColor = Brushes.Orange;
        }
        else
        {
            ConnectionColor = Brushes.Red;
        }
    }

    // Pause Button
    public void TogglePauseButton(object msg)
    {
        if (CountdownClock.Enabled)
            CountdownClock.Stop();
        else
            CountdownClock.Start();

        UpdatePauseButtonText();
    }

    public void UpdatePauseButtonText()
    {
        TimerButtonText = (this.CountdownClock.Enabled ? "Pause" : "Resume") + " Timer";
    }
}
