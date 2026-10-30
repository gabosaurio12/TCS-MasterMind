using MasterMind_Server.Contracts;
using MasterMind_Server.Repositories.Dtos;

namespace MasterMind_Server.Repositories.Interfaces
{
    interface ILobbyRepository
    {
        ChatMessageResult SendMessage(ChatMessage message);
    }
}
