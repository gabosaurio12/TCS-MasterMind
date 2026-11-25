using MasterMind_Server.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal sealed class MatchState
    {
        private const int PointsToWinMatch = 2;

        private readonly object sync = new object();
        private readonly Dictionary<string, int> pointsByUser =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> bestTriesByUser =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private int roundNumber = 1;

        public IReadOnlyDictionary<string, int> Apply(RoundResult result)
        {
            lock (sync)
            {
                result.RoundNumber = roundNumber;
                if (!string.IsNullOrEmpty(result.WinnerUsername))
                {
                    AwardPoint(result.WinnerUsername);
                    RecordWinnerTries(result);
                }

                result.Scores = result.Players.Select(BuildScore).ToArray();
                result.IsMatchOver = result.Scores.Any(score => score.Points >= PointsToWinMatch);
                return new Dictionary<string, int>(bestTriesByUser, StringComparer.OrdinalIgnoreCase);
            }
        }

        private void RecordWinnerTries(RoundResult result)
        {
            var winner = result.Players.FirstOrDefault(
                player => string.Equals(player.Username, result.WinnerUsername, StringComparison.OrdinalIgnoreCase));
            if (winner == null)
            {
                return;
            }

            bestTriesByUser.TryGetValue(winner.Username, out var bestTries);
            if (bestTries == 0 || winner.TriesUsed < bestTries)
            {
                bestTriesByUser[winner.Username] = winner.TriesUsed;
            }
        }

        private void AwardPoint(string username)
        {
            pointsByUser.TryGetValue(username, out var points);
            pointsByUser[username] = points + 1;
            roundNumber++;
        }

        private PlayerScore BuildScore(OpponentProgress player)
        {
            pointsByUser.TryGetValue(player.Username, out var points);
            return new PlayerScore { Username = player.Username, Points = points };
        }
    }
}
