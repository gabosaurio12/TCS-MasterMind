using MasterMind_Server.Contracts;
using System;
using System.ServiceModel;
using System.Threading;

namespace MasterMind_Server.Tests.TestSupport
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    internal class MatchCallbackProbe : IMatchCallbackService
    {
        private readonly ManualResetEventSlim finishedEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim resolvedEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim abandonedEvent = new ManualResetEventSlim(false);

        public OpponentProgress LastProgress { get; private set; }

        public RoundResult LastResult { get; private set; }

        public string LastAbandonedUsername { get; private set; }

        public void OnOpponentFinishedCallback(OpponentProgress progress)
        {
            LastProgress = progress;
            finishedEvent.Set();
        }

        public void OnRoundResolvedCallback(RoundResult result)
        {
            LastResult = result;
            resolvedEvent.Set();
        }

        public void OnOpponentAbandonedCallback(string username)
        {
            LastAbandonedUsername = username;
            abandonedEvent.Set();
        }

        public bool WaitForOpponentFinished(TimeSpan timeout)
        {
            return finishedEvent.Wait(timeout);
        }

        public bool WaitForRoundResolved(TimeSpan timeout)
        {
            return resolvedEvent.Wait(timeout);
        }

        public bool WaitForOpponentAbandoned(TimeSpan timeout)
        {
            return abandonedEvent.Wait(timeout);
        }
    }
}
