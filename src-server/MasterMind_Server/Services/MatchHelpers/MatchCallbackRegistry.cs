using log4net;
using MasterMind_Server.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal static class MatchCallbackRegistry
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MatchCallbackRegistry));

        private static readonly ConcurrentDictionary<string, IMatchCallbackService> callbacksByUser =
            new ConcurrentDictionary<string, IMatchCallbackService>(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<int, RoundState> roundsByRoom =
            new ConcurrentDictionary<int, RoundState>();

        private static readonly ConcurrentDictionary<int, MatchState> matchesByRoom =
            new ConcurrentDictionary<int, MatchState>();

        public static void Subscribe(string username, IMatchCallbackService callback)
        {
            callbacksByUser[username] = callback;
        }

        public static void Unsubscribe(string username, IMatchCallbackService callback)
        {
            ((ICollection<KeyValuePair<string, IMatchCallbackService>>)callbacksByUser)
                .Remove(new KeyValuePair<string, IMatchCallbackService>(username, callback));
        }

        public static void Remove(string username)
        {
            IMatchCallbackService removedCallback;
            callbacksByUser.TryRemove(username, out removedCallback);
        }

        public static void StartMatch(int roomId)
        {
            matchesByRoom[roomId] = new MatchState();
        }

        public static void ApplyRoundOutcome(int roomId, RoundResult result)
        {
            matchesByRoom.GetOrAdd(roomId, key => new MatchState()).Apply(result);
        }

        public static void OpenRound(int roomId)
        {
            roundsByRoom[roomId] = new RoundState();
        }

        public static bool TryAbandonRound(int roomId)
        {
            RoundState round;
            return roundsByRoom.TryRemove(roomId, out round) && !round.IsResolved;
        }

        public static int CountTry(int roomId, string username)
        {
            RoundState round;
            return roundsByRoom.TryGetValue(roomId, out round) ? round.CountTry(username) : 0;
        }

        public static RoundResult RecordFinish(int roomId, OpponentProgress progress)
        {
            RoundState round;
            return roundsByRoom.TryGetValue(roomId, out round) ? round.Finish(progress) : RoundState.CreatePendingResult();
        }

        public static void Notify(string username, Action<IMatchCallbackService> notify)
        {
            IMatchCallbackService callback;
            if (string.IsNullOrWhiteSpace(username) || !callbacksByUser.TryGetValue(username, out callback))
            {
                logger.WarnFormat("No match callback registered for {0}", username);
                return;
            }

            try
            {
                notify(callback);
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException
                || ex is ObjectDisposedException)
            {
                logger.WarnFormat("Match callback for {0} failed: {1}", username, ex.Message);
                Unsubscribe(username, callback);
            }
        }
    }
}
