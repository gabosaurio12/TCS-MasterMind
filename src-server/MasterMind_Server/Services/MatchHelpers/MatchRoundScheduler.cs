using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal static class MatchRoundScheduler
    {
        private static readonly ConcurrentDictionary<int, Timer> timersByRoom =
            new ConcurrentDictionary<int, Timer>();

        public static void Start(int roomId, TimeSpan delay, Action<int> onElapsed)
        {
            var timer = new Timer(state => onElapsed(roomId), null, delay, Timeout.InfiniteTimeSpan);
            Cancel(roomId);
            timersByRoom[roomId] = timer;
        }

        public static void Cancel(int roomId)
        {
            Timer timer;
            if (timersByRoom.TryRemove(roomId, out timer))
            {
                timer.Dispose();
            }
        }
    }
}
