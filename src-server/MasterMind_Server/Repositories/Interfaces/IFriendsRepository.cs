using System.Collections.Generic;

namespace MasterMind_Server.Repositories.Interfaces
{
    interface IFriendsRepository
    {
        List<Contracts.Player> GetFriendships(int playerId);

        List<Contracts.Player> GetFriendRequests(int playerId);

        Contracts.Friendship GetFriendship(int addresseeId, int requesterId);

        Contracts.Friendship GetFriendRequest(int addresseeId, int requesterId);

        string SendFriendRequest(int addresseeId, int requesterId);

        string AcceptFriendRequest(int requestId);

        string RejectFriendRequest(int requestId);

        string RemoveFriendship(string addresseeUsername, string requesterUsername);
    }
}
