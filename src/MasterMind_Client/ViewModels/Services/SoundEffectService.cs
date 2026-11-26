using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.Services
{
    public sealed class SoundEffectService : ISoundEffectService
    {
        private const string SoundsFolder = "Assets\\Sounds";
        private const double PlaybackVolume = 0.7;

        private static readonly ILog logger = LogManager.GetLogger(typeof(SoundEffectService));

        private static readonly Dictionary<SoundEffect, string> fileNames = new Dictionary<SoundEffect, string>
        {
            { SoundEffect.ButtonClick, "ButtonClick.mp3" },
            { SoundEffect.CodeTryResult, "CodeTryResult.mp3" },
            { SoundEffect.CodeTryCorrect, "CodeTryCorrect.mp3" },
            { SoundEffect.MessageReceived, "MessageReceived.mp3" },
            { SoundEffect.PinSelect, "PinSelect.mp3" },
            { SoundEffect.TimeIsOver, "TimeIsOver.mp3" }
        };

        public bool IsMuted => Properties.Settings.Default.SfxMuted;

        public void Play(SoundEffect effect)
        {
            if (IsMuted)
            {
                return;
            }

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SoundsFolder, fileNames[effect]);

            if (!File.Exists(path))
            {
                logger.WarnFormat("Sound effect not found: {0}", path);
                return;
            }

            var player = new MediaPlayer { Volume = PlaybackVolume };
            player.MediaEnded += (sender, e) => player.Close();
            player.MediaFailed += (sender, e) => OnMediaFailed(player, e);
            player.Open(new Uri(path, UriKind.Absolute));
            player.Play();
        }

        public void ToggleMute()
        {
            Properties.Settings.Default.SfxMuted = !IsMuted;
            Properties.Settings.Default.Save();
        }

        private static void OnMediaFailed(MediaPlayer player, ExceptionEventArgs e)
        {
            logger.Error("Sound effect failed to play.", e.ErrorException);
            player.Close();
        }
    }
}
