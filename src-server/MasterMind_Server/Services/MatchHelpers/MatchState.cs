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
        private int roundNumber = 1;

        public void Apply(RoundResult result)
        {
            lock (sync)
            {
                result.RoundNumber = roundNumber;
                if (!string.IsNullOrEmpty(result.WinnerUsername))
                {
                    AwardPoint(result.WinnerUsername);
                }

                result.Scores = result.Players.Select(BuildScore).ToArray();
                result.IsMatchOver = result.Scores.Any(score => score.Points >= PointsToWinMatch);
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
