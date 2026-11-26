using MasterMind_Client.ViewModels.Base;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers
{
    public class MatchClockViewModel : ViewModelBase, IDisposable
    {
        private const int TickMilliseconds = 100;
        private const int MillisecondsPerSecond = 1000;
        private const int SecondsPerMinute = 60;

        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(TickMilliseconds)
        };

        private readonly Stopwatch stopwatch = new Stopwatch();
        private int durationMs;
        private string text = FormatSeconds(0);

        public MatchClockViewModel()
        {
            timer.Tick += OnTick;
        }

        public event EventHandler Elapsed;

        public string Text
        {
            get => text;
            private set => SetProperty(ref text, value);
        }

        public bool IsRunning => timer.IsEnabled;

        public void Start(int remainingMs)
        {
            durationMs = remainingMs;
            stopwatch.Restart();
            timer.Start();
            Refresh();
        }

        public void Stop()
        {
            timer.Stop();
            stopwatch.Stop();
        }

        public void Reset()
        {
            Stop();
            Text = FormatSeconds(0);
        }

        public void Dispose()
        {
            Stop();
            timer.Tick -= OnTick;
        }

        private int GetRemainingMs()
        {
            return Math.Max(0, durationMs - (int)stopwatch.ElapsedMilliseconds);
        }

        private void OnTick(object sender, EventArgs e)
        {
            Refresh();
            if (GetRemainingMs() == 0)
            {
                Stop();
                Elapsed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Refresh()
        {
            int seconds = (GetRemainingMs() + MillisecondsPerSecond - 1) / MillisecondsPerSecond;
            Text = FormatSeconds(seconds);
        }

        private static string FormatSeconds(int totalSeconds)
        {
            return $"{totalSeconds / SecondsPerMinute:00}:{totalSeconds % SecondsPerMinute:00}";
        }
    }
}
