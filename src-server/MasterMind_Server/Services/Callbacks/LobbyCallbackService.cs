using MasterMind_Server.Contracts;
using System;
using System.ServiceModel;

namespace MasterMind_Server.Services.Callbacks
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LobbyCallbackService : ILobbyCallbackService
    {


        public void OnChangePlayerStatusCallback(PlayerStatus status)
        {
            
        }

        public void OnMessageReceivedCallback(ChatMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
