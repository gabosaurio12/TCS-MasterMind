using MasterMind_Client.MatchService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers
{
    public class MatchScoreViewModel : ViewModelBase
    {
        private IReadOnlyList<bool> ownLights = BuildLights(0);
        private IReadOnlyList<bool> opponentLights = BuildLights(0);

        public IReadOnlyList<bool> OwnLights
        {
            get => ownLights;
            private set => SetProperty(ref ownLights, value);
        }

        public IReadOnlyList<bool> OpponentLights
        {
            get => opponentLights;
            private set => SetProperty(ref opponentLights, value);
        }

        public void Update(IEnumerable<PlayerScore> scores, string ownUsername)
        {
            var allScores = scores?.ToList() ?? new List<PlayerScore>();
            var own = allScores.FirstOrDefault(score => IsSameUser(score.Username, ownUsername));
            var opponent = allScores.FirstOrDefault(score => !IsSameUser(score.Username, ownUsername));
            OwnLights = BuildLights(own?.Points ?? 0);
            OpponentLights = BuildLights(opponent?.Points ?? 0);
        }

        private static bool IsSameUser(string first, string second)
        {
            return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
        }

        private static IReadOnlyList<bool> BuildLights(int litCount)
        {
            return Enumerable.Range(0, MatchRules.PointsToWinMatch).Select(index => index < litCount).ToList();
        }
    }
}
