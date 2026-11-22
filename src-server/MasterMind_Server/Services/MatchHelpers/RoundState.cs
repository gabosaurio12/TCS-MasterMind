using MasterMind_Server.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal sealed class RoundState
    {
        private const int PlayersPerRound = 2;

        private readonly object sync = new object();
        private readonly Dictionary<string, int> triesByUser =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, OpponentProgress> finishedByUser =
            new Dictionary<string, OpponentProgress>(StringComparer.OrdinalIgnoreCase);

        public bool IsResolved { get; private set; }

        public static RoundResult CreatePendingResult()
        {
            return new RoundResult { Players = new OpponentProgress[0] };
        }

        public static bool IsPending(RoundResult result)
        {
            return result.Players.Length == 0;
        }

        public int CountTry(string username)
        {
            lock (sync)
            {
                if (finishedByUser.ContainsKey(username))
                {
                    return 0;
                }

                triesByUser.TryGetValue(username, out var tries);
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
                    return CreatePendingResult();
                }

                IsResolved = true;
                return BuildResult();
            }
        }

        private RoundResult BuildResult()
        {
            var players = finishedByUser.Values.ToArray();
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
