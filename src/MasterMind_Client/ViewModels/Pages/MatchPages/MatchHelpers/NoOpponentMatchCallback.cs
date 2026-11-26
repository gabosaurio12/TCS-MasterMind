using MasterMind_Client.MatchService;
using System.ServiceModel;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class NoOpponentMatchCallback : IMatchServiceCallback
    {
        public void OnOpponentFinishedCallback(OpponentProgress progress)
        {
        }

        public void OnRoundResolvedCallback(RoundResult result)
        {
        }

        public void OnOpponentAbandonedCallback(string username)
        {
        }

        public void OnTimeTrialStartedCallback(int remainingMs)
        {
        }
    }
}
