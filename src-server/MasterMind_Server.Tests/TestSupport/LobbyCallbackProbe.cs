using MasterMind_Server.Contracts;
using System;
using System.Threading;

namespace MasterMind_Server.Tests.TestSupport
{
    internal class LobbyCallbackProbe : ILobbyCallbackService
    {
        private readonly ManualResetEventSlim messageReceivedEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim statusChangedEvent = new ManualResetEventSlim(false);

        public ChatMessage LastReceivedMessage { get; private set; }

        public PlayerStatus LastStatusChange { get; private set; }

        public void OnMessageReceivedCallback(ChatMessage message)
        {
            LastReceivedMessage = message;
            messageReceivedEvent.Set();
        }

        public void OnChangePlayerStatusCallback(PlayerStatus status)
        {
            LastStatusChange = status;
            statusChangedEvent.Set();
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return messageReceivedEvent.Wait(timeout);
        }

        public bool WaitForStatusChange(TimeSpan timeout)
        {
            return statusChangedEvent.Wait(timeout);
        }

        public void Reset()
        {
            LastReceivedMessage = null;
            LastStatusChange = null;
            messageReceivedEvent.Reset();
            statusChangedEvent.Reset();
        }
    }
}
