using MasterMind_Client.MatchService;
using System.ServiceModel;

namespace MasterMind_Client.ViewModels.Pages.MatchPages
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class NoOpMatchCallback : IMatchServiceCallback
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
    }
}
