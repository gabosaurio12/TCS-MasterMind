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

        public static void Subscribe(string username, IMatchCallbackService callback)
        {
            callbacksByUser[username] = callback;
        }

        public static void Unsubscribe(string username, IMatchCallbackService callback)
        {
            ((ICollection<KeyValuePair<string, IMatchCallbackService>>)callbacksByUser)
                .Remove(new KeyValuePair<string, IMatchCallbackService>(username, callback));
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
            return roundsByRoom.TryGetValue(roomId, out round) ? round.Finish(progress) : null;
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

        private class RoundState
        {
            private const int PlayersPerRound = 2;

            private readonly object sync = new object();
            private readonly Dictionary<string, int> triesByUser =
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, OpponentProgress> finishedByUser =
                new Dictionary<string, OpponentProgress>(StringComparer.OrdinalIgnoreCase);

            public bool IsResolved { get; private set; }

            public int CountTry(string username)
            {
                lock (sync)
                {
                    if (finishedByUser.ContainsKey(username))
                    {
                        return 0;
                    }

                    int tries;
                    triesByUser.TryGetValue(username, out tries);
                    triesByUser[username] = tries + 1;
                    return tries + 1;
                }
            }

            public RoundResult Finish(OpponentProgress progress)
            {
                lock (sync)
                {
                    finishedByUser[progress.Username] = progress;
                    if (finishedByUser.Count < PlayersPerRound || IsResolved)
                    {
                        return null;
                    }

                    IsResolved = true;
                    return BuildResult();
                }
            }

            private RoundResult BuildResult()
            {
                var players = new List<OpponentProgress>(finishedByUser.Values).ToArray();
                return new RoundResult
                {
                    WinnerUsername = DetermineWinner(players[0], players[1]),
                    Players = players
                };
            }

            private static string DetermineWinner(OpponentProgress first, OpponentProgress second)
            {
                if (first.Solved && second.Solved)
                {
                    return DetermineFasterWinner(first, second);
                }

                if (first.Solved || second.Solved)
                {
                    return first.Solved ? first.Username : second.Username;
                }

                return null;
            }

            private static string DetermineFasterWinner(OpponentProgress first, OpponentProgress second)
            {
                if (first.TriesUsed == second.TriesUsed)
                {
                    return null;
                }

                return first.TriesUsed < second.TriesUsed ? first.Username : second.Username;
            }
        }
    }
}
