using log4net;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Enums;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Runtime.InteropServices;

namespace MasterMind_Server.Repositories
{
    internal class FriendsRepository
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(PlayerRepository));
        private readonly static MasterMindEntities Context = new MasterMindEntities(true);

        public static List<Contracts.Player> GetFriendships(int playerId)
        {
            try
            {
                int acceptedStatusId = Context.RequestStatusCatalog
                    .FirstOrDefault(rq => rq.status == FriendshipStatusEnum.Accepted
                    .ToString()).request_status_id;

                var friendships = Context.Friendship.Where(friendship => (
                    friendship.requester_id == playerId ||
                    friendship.addressee_id == playerId) &&
                    friendship.status_id == acceptedStatusId).ToList();

                List<int> friendsIds = GetFriendsIds(friendships, playerId);

                List<Contracts.Player> friends = new List<Contracts.Player>();

                foreach (var friendId in friendsIds)
                {
                    var friend = PlayerRepository.GetPlayerById(friendId);
                    if (friend != null)
                    {

                        friends.Add(PlayerEntityToPlayerContract(friend));
                    }
                }

                return friends;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw ex;
            }
        }

        private static Contracts.Player PlayerEntityToPlayerContract(Player player)
        {
            return new Contracts.Player
            {
                PlayerId = player.player_id,
                Username = player.username,
                Email = player.email,
                AvatarPath = player.avatar_path
            };
        }


        public static List<Contracts.Player> GetFriendRequests(int playerId)
        {
            try
            {
                int pendingStatusId = Context.RequestStatusCatalog
                        .FirstOrDefault(rq => rq.status == FriendshipStatusEnum.Pending.ToString()).request_status_id;

                var friendships = Context.Friendship.Where(friendship => friendship.addressee_id == playerId
                    && friendship.status_id == pendingStatusId).ToList();

                List<int> friendsIds = GetFriendsIds(friendships, playerId);

                var friends = GetFriendshipsFromIds(friendsIds);

                return friends;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw ex;
            }
        }

        private static List<int> GetFriendsIds(List<Friendship> friendships, int playerId)
        {
            List<int> friendsIds = new List<int>();

            foreach (var friendship in friendships)
            {
                int friendId;
                if (friendship.addressee_id != playerId)
                {
                    friendId = friendship.addressee_id;
                }
                else
                {
                    friendId = friendship.requester_id;
                }

                friendsIds.Add(friendId);
            }

            return friendsIds;
        }

        private static List<Contracts.Player> GetFriendshipsFromIds(List<int> friendsIds)
        {
            List<Contracts.Player> friends = new List<Contracts.Player>();

            foreach (var friendId in friendsIds)
            {
                var friendEntity = PlayerRepository.GetPlayerById(friendId);
                if (friendEntity != null)
                {
                    var friend = new Contracts.Player
                    {
                        PlayerId = friendEntity.player_id,
                        Username = friendEntity.username,
                        Email = friendEntity.email,
                        AvatarPath = friendEntity.avatar_path
                    };

                    friends.Add(friend);
                }
            }

            return friends;
        }

        public static Contracts.Friendship GetFriendship(int addresseeId, int requesterId)
        {
            try
            {
                int acceptedStatusId = Context.RequestStatusCatalog
                    .FirstOrDefault(rq => rq.status == FriendshipStatusEnum.Accepted
                    .ToString()).request_status_id;

                var requestEntity = Context.Friendship.FirstOrDefault(
                                req => req.requester_id == requesterId &&
                                req.addressee_id == addresseeId &&
                                req.status_id == acceptedStatusId);

                var request = new Contracts.Friendship
                {
                    FriendshipId = 0
                };

                if (requestEntity != null)
                {
                    request = new Contracts.Friendship
                    {
                        FriendshipId = requestEntity.friendship_id,
                        AddresseeUsername = PlayerRepository.GetPlayerById(requestEntity.addressee_id).username,
                        RequesterUsername = PlayerRepository.GetPlayerById(requestEntity.requester_id).username,
                        Status = FriendshipStatusEnum.Accepted.ToString()
                    };
                }

                return request;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw ex;
            }
        }

        public static Contracts.Friendship GetFriendRequest(int addresseeId, int requesterId)
        {
            try
            {
                int pendingStatusId = Context.RequestStatusCatalog
                    .FirstOrDefault(rq => rq.status == FriendshipStatusEnum.Pending
                    .ToString()).request_status_id;

                var requestEntity = Context.Friendship.FirstOrDefault(
                                req => req.requester_id == requesterId &&
                                req.addressee_id == addresseeId &&
                                req.status_id == pendingStatusId);

                var request = new Contracts.Friendship
                {
                    FriendshipId = 0
                };

                if (requestEntity != null)
                {
                    request = new Contracts.Friendship
                    {
                        FriendshipId = requestEntity.friendship_id,
                        AddresseeUsername = PlayerRepository.GetPlayerById(requestEntity.addressee_id).username,
                        RequesterUsername = PlayerRepository.GetPlayerById(requestEntity.requester_id).username,
                        Status = FriendshipStatusEnum.Accepted.ToString()
                    };
                }

                return request;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw ex;
            }
        }
    }
}
