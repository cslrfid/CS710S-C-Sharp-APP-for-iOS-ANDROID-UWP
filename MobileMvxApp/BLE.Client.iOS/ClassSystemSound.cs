using System;
using System.Diagnostics;
using System.IO;
using AVFoundation;
using Foundation;
using Xamarin.Forms;

using AudioToolbox;

[assembly: Dependency(typeof(BLE.Client.iOS.SystemSound_iOS))]
namespace BLE.Client.iOS
{
    public class SystemSound_iOS : BLE.Client.ISystemSound
    {
        static NSError err;
        private AVAudioPlayer backgroundMusic3 = new AVAudioPlayer(new NSUrl("Sounds/316854__kwahmah-02__2-pop.wav"), "wav", out err);
        private AVAudioPlayer backgroundMusic4 = new AVAudioPlayer(new NSUrl("Sounds/beep-07.wav"), "wav", out err);
        private AVAudioPlayer backgroundMusic5 = new AVAudioPlayer(new NSUrl("Sounds/245952__kwahmah-02__1khz.wav"), "wav", out err);
        public bool MusicOn { get; set; } = true;
        public float MusicVolume { get; set; } = 0.5f;

        public SystemSound_iOS ()
        {
            backgroundMusic3.NumberOfLoops = 0;
            backgroundMusic4.NumberOfLoops = 0;
            backgroundMusic5.NumberOfLoops = 0;
            backgroundMusic3.Volume = MusicVolume;
            backgroundMusic4.Volume = MusicVolume;
            backgroundMusic5.Volume = MusicVolume;

            ActivateAudioSession();
        }

        public void SystemSound(int id)
        {
            switch (id)
            {
                case -1:
                    backgroundMusic3.Stop();
                    backgroundMusic4.Stop();
                    backgroundMusic5.Stop();
                    break;

                case 2:
                    backgroundMusic3.Stop();
                    backgroundMusic4.Stop();
                    backgroundMusic5.Stop();
                    backgroundMusic3.Play();
                    break;

                case 1:
                case 3:
                    backgroundMusic3.Stop();
                    backgroundMusic4.Stop();
                    backgroundMusic5.Stop();
                    backgroundMusic4.Play();
                    break;

                case 4:
                    backgroundMusic3.Stop();
                    backgroundMusic4.Stop();

                    if (!backgroundMusic5.Playing)
                    {
                        backgroundMusic5.Play();
                    }
                    break;
            }
        }

        public void ActivateAudioSession()
        {
            // Initialize Audio
            var session = AVAudioSession.SharedInstance();
            session.SetCategory(AVAudioSessionCategory.Ambient);

            session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers);

            session.SetActive(true);
        }

        public void DeactivateAudioSession()
        {
            var session = AVAudioSession.SharedInstance();
            session.SetActive(false);
        }

        public void ReactivateAudioSession()
        {
            var session = AVAudioSession.SharedInstance();
            session.SetActive(true);
        }
    }
}
