using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Timers;
using TwitchTTSUser.Models;

namespace TwitchTTSUser.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ConfigData Config { get; private set; }
    public TwitchService Twitch { get; private set; }
    private TTSService TTS;
    private SFXService SFX;
    private FileWriter OBSTextData;

    private Timer CountdownClock;
    private int CurrentTime = 0;

    [ObservableProperty]
    public IBrush connectionColor = Brushes.White;

    [ObservableProperty]
    public string connectionStatus = "Disconnected";

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
        OBSTextData = new FileWriter();
        Twitch = new TwitchService(Config);
        TTS = new TTSService(Config);

        // Shows a countdown clock on screen for each person's turn
        CountdownClock = new Timer();
        CountdownClock.Elapsed += new ElapsedEventHandler(UpdateClock);
        CountdownClock.Interval = 1000;

        Twitch.OnMessageSent = HandleUserMessage;
        Twitch.OnNewSelectedUser = HandleNewSelectedUser;
        Twitch.OnConnectionStatus = HandleConnectionStatus;
    }

    /*** CLOCK ***/
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
            CurrentTime = 0;

            if (Config.AutoChooseNextPerson)
                Twitch.PickUser(); // If successful, will callback to HandleNewSelectedUser
        }
    }

    private bool IsClockRunning() => CountdownClock.Enabled;

    /*** IO FUNCTIONALITY ***/
    private void HandleUserMessage(string IncomingMessage)
    {
        if (Config.InteractOnlyIfClockRunning && !IsClockRunning())
            return;

        OBSTextData.WriteMessage(IncomingMessage);
        TTS.SayMessage(IncomingMessage);
    }

    /*** MAIN LOGIC ***/
    private void HandleConnectionStatus(bool Connected)
    {
        ConnectionColor = (Connected) ? Brushes.Green : Brushes.Red;
        ConnectionStatus = (Connected) ? "Connected!" : "Connection Error!";
        IsConnected = Connected;
    }

    private void HandleNewSelectedUser(string NewUsername)
    {
        SelectedUser = NewUsername;
        UserTime = string.Empty;
        OBSTextData.WriteMessage(string.Empty);
        OBSTextData.WriteUsername(NewUsername);

        if (!string.IsNullOrEmpty(NewUsername))
        {
            UserSelected = true;
            TTS.ChooseRandomVoiceSetting();
            TTS.SayUsername(NewUsername);

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
        if (Twitch.ConnectToChannel())
        {
            // Set our state to Orange. It will tell us what further thing to do.
            ConnectionColor = Brushes.Orange;
            ConnectionStatus = "Connecting...";
        }
        else
        {
            ConnectionColor = Brushes.Red;
            ConnectionStatus = "Connection Error!";
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
        TimerButtonText = (IsClockRunning() ? "Pause" : "Resume") + " Timer";
    }

    // Clear User Queue Button
    public void ClearUserButton(object msg)
    {
        OBSTextData.ClearFiles();
        Twitch.ClearUser();
    }

    // Pick User Button
    public void PickUserButton(object msg)
    {
        Twitch.PickUser();
    }
}
