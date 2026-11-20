using MasterMind_Client.MatchService;
using System;
using System.ServiceModel;
using System.Windows;

namespace MasterMind_Client.ViewModels.Pages.MatchPages
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class MatchCallbackHandler : IMatchServiceCallback
    {
        public event EventHandler<OpponentProgress> OpponentFinished;

        public event EventHandler<RoundResult> RoundResolved;

        public event EventHandler<string> OpponentAbandoned;

        public void OnOpponentFinishedCallback(OpponentProgress progress)
        {
            RaiseOnUiThread(OpponentFinished, progress);
        }

        public void OnRoundResolvedCallback(RoundResult result)
        {
            RaiseOnUiThread(RoundResolved, result);
        }

        public void OnOpponentAbandonedCallback(string username)
        {
            RaiseOnUiThread(OpponentAbandoned, username);
        }

        private void RaiseOnUiThread<T>(EventHandler<T> handler, T argument)
        {
            Application.Current?.Dispatcher.BeginInvoke((Action)(() => handler?.Invoke(this, argument)));
        }
    }
}
