using System;
using System.Speech.Synthesis;
using System.Runtime.InteropServices;

namespace TwitchTTSUser.Base
{
    public class TTSService
    {
        private SpeechSynthesizer? Synth = null;

        public TTSService() 
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Speech Synthesis is not available on this platform!");
                return;
            }

            Synth = new SpeechSynthesizer();
            Synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Teen);
            Synth.Volume = 100;
            Synth.Rate = 2;

            Synth.SetOutputToDefaultAudioDevice();
        }

        public void SayMessage(string message)
        {
            if (Synth != null)
            {
                string truncatedMessage = message.Substring(0, Math.Min(message.Length, 400));
#pragma warning disable CA1416 // Validate platform compatibility
                Synth.SpeakAsync(truncatedMessage);
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }
    }
}
