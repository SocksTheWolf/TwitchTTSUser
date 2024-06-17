﻿using Avalonia.Media;
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
                Twitch.PickUser();
        }
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
            CurrentTime = Config.MaxSelectedTime;
            CountdownClock.Start();
            TTS.ChooseRandomVoiceSetting();
        }
        else
        {
            CountdownClock.Stop();
        }
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
