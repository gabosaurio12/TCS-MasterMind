using MasterMind_Server.Contracts;
using System;
using System.Threading;

namespace MasterMind_Server.Tests.TestSupport
{
    internal class FriendsCallbackProbe : IFriendsCallbackService
    {
        private readonly ManualResetEventSlim changedEvent = new ManualResetEventSlim(false);

        public void OnFriendshipStatusChanged()
        {
            changedEvent.Set();
        }

        public void Reset()
        {
            changedEvent.Reset();
        }

        public bool WaitForChange(TimeSpan timeout)
        {
            return changedEvent.Wait(timeout);
        }
    }
}
