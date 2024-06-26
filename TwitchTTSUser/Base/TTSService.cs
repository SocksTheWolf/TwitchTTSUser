﻿using System;
using System.Threading;
using System.Speech.Synthesis;
using System.Runtime.InteropServices;
using System.Linq;

namespace TwitchTTSUser.Base
{
#pragma warning disable CA1416 // Validate platform compatibility
    public class TTSService
    {
        private SpeechSynthesizer? Synth = null;
        private Random rng = new Random();
        private int RateRange = 5;

        public TTSService(int VoiceVolume, int VoiceRate) 
        {
            if (!IsSupported())
            {
                Console.WriteLine("Speech Synthesis is not available on this platform!");
                return;
            }

            Synth = new SpeechSynthesizer();
            Synth.Volume = VoiceVolume;
            RateRange = VoiceRate;
            Synth.SetOutputToDefaultAudioDevice();
            ChooseRandomVoiceSetting();
        }

        public void ChooseRandomVoiceSetting()
        {
            if (Synth == null)
                return;

            // Remove the NotSet value as a valid value
            Array VoiceAges = Enum.GetValues<VoiceAge>().Skip(1).ToArray();
            Array VoiceGenders = Enum.GetValues<VoiceGender>().Skip(1).ToArray();
            if (VoiceAges.Length == 0 || VoiceGenders.Length == 0)
            {
                Synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Teen);
                return;
            }

            Synth.Rate = rng.Next(-RateRange, RateRange);
            // These already have checks above to prevent unpacking null objects.
#pragma warning disable CS8605
            VoiceAge SelectedAge = (VoiceAge)VoiceAges.GetValue(rng.Next(VoiceAges.Length));
            VoiceGender SelectedGender = (VoiceGender)VoiceGenders.GetValue(rng.Next(VoiceGenders.Length));
#pragma warning restore CS8605
            Synth.SelectVoiceByHints(SelectedGender, SelectedAge);
        }

        public void SayMessage(string message)
        {
            if (Synth != null)
            {
                string truncatedMessage = message.Substring(0, Math.Min(message.Length, 400));
                // Arbitrary delay added so that the file data will show up in OBS properly
                // Before the TTS is said.
                Thread.Sleep(2000);

                Synth.SpeakAsync(truncatedMessage);
            }
        }

        public static bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
#pragma warning restore CA1416 // Validate platform compatibility
}
