using MasterMind_Client.TempData.Enum;
using System.Collections.Generic;
using System.Linq;

namespace MasterMind_Client.TempData
{
    public static class FriendshipService
    {
        public static List<TempFriendship> Friendships { get; set; } = new List<TempFriendship>();
        private static int friendshipsId = 0;

        public static RequestResult SendFrienshipRequest(int requesterId, int addresseeId)
        {
            var request = Friendships.FirstOrDefault(
                r => r.RequesterId == requesterId &&
                r.AddreseeId == addresseeId &&
                r.Status == Enum.RequestStatusEnum.Pendant);

            if (request == null)
            {
                Friendships.Add(
                    new TempFriendship
                    {
                        Id = ++friendshipsId,
                        RequesterId = requesterId,
                        AddreseeId = addresseeId,
                        Status = RequestStatusEnum.Pendant
                    });

                return RequestResult.Success;
            }
            else
            {
                if (request.Status == RequestStatusEnum.Pendant)
                    return RequestResult.RequestIsPendant;
            }

            return RequestResult.Error;
        }

        public static RequestResult AcceptFriendRequest(int requestId)
        {
            var request = Friendships.FirstOrDefault(
                r => r.Id == requestId &&
                r.Status == RequestStatusEnum.Pendant);

            if (request != null)
            {
                request.Status = RequestStatusEnum.Accepted;
                return RequestResult.Success;
            }

            return RequestResult.Error;
        }

        public static RequestResult RejectFriendRequest(int requestId)
        {
            var request = Friendships.FirstOrDefault(
                r => r.Id == requestId &&
                r.Status == RequestStatusEnum.Pendant);

            if (request != null)
            {
                Friendships.Remove(request);
                return RequestResult.Success;
            }

            return RequestResult.Error;
        }

        public static List<TempPlayer> GetFriendRequests(int playerId)
        {
            var friendships = Friendships.Where(f => f.AddreseeId == playerId &&
                f.Status == RequestStatusEnum.Pendant).ToList();

            List<int> friendsIds = new List<int>();

            foreach (var i in friendships)
            {
                friendsIds.Add(i.AddreseeId != playerId ? i.AddreseeId : i.RequesterId);
            }

            List<TempPlayer> friends = new List<TempPlayer>();

            foreach (var i in friendsIds)
            {
                var friend = TempAuthService.Players.FirstOrDefault(p => p.Id == i);
                if (friend != null)
                {
                    friends.Add(friend);
                }
            }

            return friends;
        }

        public static List<TempPlayer> GetFrienships(int playerId)
        {
            var friendships = Friendships.Where(f => (
                f.RequesterId == playerId ||
                f.AddreseeId == playerId) &&
                f.Status == RequestStatusEnum.Accepted).ToList();

            List<int> friendsIds = new List<int>();

            foreach (var i in friendships)
            {
                friendsIds.Add(i.AddreseeId != playerId ? i.AddreseeId : i.RequesterId);
            }

            List<TempPlayer> friends = new List<TempPlayer>();

            foreach (var i in friendsIds)
            {
                var friend = TempAuthService.Players.FirstOrDefault(p => p.Id == i);
                if (friend != null)
                {
                    friends.Add(friend);
                }
            }

            return friends;
        }

        public static TempFriendship GetFriendRequest(int requesterId, int addresseeId)
        {
            var request = Friendships.FirstOrDefault(
                r => r.RequesterId == requesterId &&
                r.AddreseeId == addresseeId &&
                r.Status == RequestStatusEnum.Pendant);

            if (request != null)
            {
                return request;
            }

            return null;
        }

        public static TempFriendship GetFriendship(int requesterId, int addresseeId)
        {
            var request = Friendships.FirstOrDefault(
                r => r.RequesterId == requesterId &&
                r.AddreseeId == addresseeId &&
                r.Status == RequestStatusEnum.Accepted);

            if (request != null)
            {
                return request;
            }

            return null;
        }
    }
}
