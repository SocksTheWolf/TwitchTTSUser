using Avalonia.Platform;
using NAudio.Wave;
using System;

namespace TwitchTTSUser.Models
{
    public class SFXService
    {
        private WaveOutEvent output;
        private WaveFileReader audioFile;
        private bool AudioPlaying = false;

        public SFXService()
        {
            output = new WaveOutEvent();
            output.PlaybackStopped += OnStoppedAudio;

            // This is baked into the binary.          
            audioFile = new WaveFileReader(AssetLoader.Open(new Uri("avares://TwitchTTSUser/Assets/sound.wav")));
            output.Init(audioFile);
        }

        ~SFXService()
        {
            output.Dispose();
            audioFile.Dispose();
        }

        public void PlayAudio()
        {
            if (AudioPlaying)
                return;

            output.Play();
            AudioPlaying = true;
        }

        private void OnStoppedAudio(object? sender, StoppedEventArgs args)
        {
            audioFile.Position = 0;
            AudioPlaying = false;
        }
    }
}
