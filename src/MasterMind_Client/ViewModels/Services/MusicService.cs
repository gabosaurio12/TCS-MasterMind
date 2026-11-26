using log4net;
using System;
using System.IO;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.Services
{
    public sealed class MusicService : IMusicService
    {
        private const string TracksFolder = "Assets\\Tracks";
        private const string TrackFileName = "Main-SpyThemes.mp3";
        private const double PlaybackVolume = 0.5;

        private static readonly ILog logger = LogManager.GetLogger(typeof(MusicService));

        private readonly MediaPlayer player = new MediaPlayer();

        public MusicService()
        {
            player.Volume = PlaybackVolume;
            player.MediaEnded += OnMediaEnded;
            player.MediaFailed += OnMediaFailed;
        }

        public bool IsMuted => Properties.Settings.Default.MusicMuted;

        public void Play()
        {
            var trackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TracksFolder, TrackFileName);

            if (!File.Exists(trackPath))
            {
                logger.WarnFormat("Background track not found: {0}", trackPath);
                return;
            }

            player.Open(new Uri(trackPath, UriKind.Absolute));
            ApplyPlaybackState();
        }

        public void ToggleMute()
        {
            Properties.Settings.Default.MusicMuted = !IsMuted;
            Properties.Settings.Default.Save();
            ApplyPlaybackState();
        }

        public void Stop()
        {
            player.Close();
        }

        private void ApplyPlaybackState()
        {
            if (IsMuted)
            {
                player.Pause();
            }
            else
            {
                player.Play();
            }
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        private static void OnMediaFailed(object sender, ExceptionEventArgs e)
        {
            logger.Error("Background music failed to play.", e.ErrorException);
        }
    }
}
