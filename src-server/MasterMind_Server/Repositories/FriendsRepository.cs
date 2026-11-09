using log4net;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;

namespace MasterMind_Server.Repositories
{
    internal class FriendsRepository : IFriendsRepository
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(PlayerRepository));
        private readonly MasterMindEntities Context = new MasterMindEntities(true);
        private readonly PlayerRepository playerRepository = new PlayerRepository();

        public List<Contracts.Player> GetFriendships(int playerId)
        {
            try
            {
                int acceptedStatusId = GetAcceptedStatusId();

                List<Contracts.Player> friends = new List<Contracts.Player>();

                if (acceptedStatusId > 0)
                {
                    var friendships = Context.Friendship.Where(friendship => (
                        friendship.requester_id == playerId ||
                        friendship.addressee_id == playerId) &&
                        friendship.status_id == acceptedStatusId)
                        .ToList();

                    List<int> friendsIds = GetFriendsIds(friendships, playerId);


                    foreach (var friendId in friendsIds)
                    {
                        var friend = playerRepository.GetPlayerById(friendId);
                        if (friend != null)
                        {

                            friends.Add(PlayerEntityToPlayerContract(friend));
                        }
                    }
                }

                return friends;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        private int GetAcceptedStatusId()
        {
            var acceptedStatusEntity = Context.RequestStatusCatalog
                    .FirstOrDefault(rq => rq.status == FriendshipStatus.Accepted
                    .ToString());

            int id = 0;

            if (acceptedStatusEntity != null)
            {
                id = acceptedStatusEntity.request_status_id;
            }

            return id;
        }

        private int GetPendingStatusId()
        {
            var pendingStatusEntity = Context.RequestStatusCatalog
                        .FirstOrDefault(rq => rq.status == FriendshipStatus.Pending.ToString());

            int id = 0;

            if (pendingStatusEntity != null)
            {
                id = pendingStatusEntity.request_status_id;
            }

            return id;
        }


        private Contracts.Player PlayerEntityToPlayerContract(Player player)
        {
            return new Contracts.Player
            {
                PlayerId = player.player_id,
                Username = player.username,
                Email = player.email,
                AvatarPath = player.avatar_path
            };
        }


        public List<Contracts.Player> GetFriendRequests(int playerId)
        {
            try
            {
                int pendingStatusId = GetPendingStatusId();

                List<Contracts.Player> friends = new List<Contracts.Player>();

                if (pendingStatusId > 0)
                {
                    var friendships = Context.Friendship.Where(friendship => friendship.addressee_id == playerId
                    && friendship.status_id == pendingStatusId).ToList();

                    List<int> friendsIds = GetFriendsIds(friendships, playerId);

                    friends = GetFriendshipsFromIds(friendsIds);
                }

                return friends;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
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

        private List<Contracts.Player> GetFriendshipsFromIds(List<int> friendsIds)
        {
            List<Contracts.Player> friends = new List<Contracts.Player>();

            foreach (var friendId in friendsIds)
            {
                var friendEntity = playerRepository.GetPlayerById(friendId);
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

        public Contracts.Friendship GetFriendship(int addresseeId, int requesterId)
        {
            try
            {
                int acceptedStatusId = GetAcceptedStatusId();

                var request = new Contracts.Friendship
                {
                    FriendshipId = 0
                };

                if (acceptedStatusId > 0)
                {
                    var requestEntity = Context.Friendship.FirstOrDefault(
                                req => req.requester_id == requesterId &&
                                req.addressee_id == addresseeId &&
                                req.status_id == acceptedStatusId);

                    if (requestEntity != null)
                    {
                        request = new Contracts.Friendship
                        {
                            FriendshipId = requestEntity.friendship_id,
                            AddresseeUsername = playerRepository.GetPlayerById(requestEntity.addressee_id).username,
                            RequesterUsername = playerRepository.GetPlayerById(requestEntity.requester_id).username,
                            Status = FriendshipStatus.Accepted.ToString()
                        };
                    }
                }

                return request;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public Contracts.Friendship GetFriendRequest(int addresseeId, int requesterId)
        {
            try
            {
                int pendingStatusId = GetPendingStatusId();

                var request = new Contracts.Friendship
                {
                    FriendshipId = 0
                };

                if (pendingStatusId > 0)
                {
                    var requestEntity = Context.Friendship.FirstOrDefault(
                                req => req.requester_id == requesterId &&
                                req.addressee_id == addresseeId &&
                                req.status_id == pendingStatusId);

                    if (requestEntity != null)
                    {
                        request = new Contracts.Friendship
                        {
                            FriendshipId = requestEntity.friendship_id,
                            AddresseeUsername = playerRepository.GetPlayerById(requestEntity.addressee_id).username,
                            RequesterUsername = playerRepository.GetPlayerById(requestEntity.requester_id).username,
                            Status = FriendshipStatus.Pending.ToString()
                        };
                    }
                }

                return request;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public string SendFriendRequest(int addresseeId, int requesterId)
        {
            try
            {
                if (addresseeId == requesterId)
                {
                    return RepositoriesMessages.Error.ToString();
                }

                int pendingStatusId = GetPendingStatusId();
                int acceptedStatusId = GetAcceptedStatusId();

                if (pendingStatusId > 0 && acceptedStatusId > 0)
                {
                    var existingRequest = Context.Friendship.FirstOrDefault(req =>
                    (req.requester_id == requesterId && req.addressee_id == addresseeId) ||
                    (req.requester_id == addresseeId && req.addressee_id == requesterId));

                    if (existingRequest != null)
                    {
                        if (existingRequest.status_id == pendingStatusId)
                        {
                            return RepositoriesMessages.Pending.ToString();
                        }

                        if (existingRequest.status_id == acceptedStatusId)
                        {
                            return RepositoriesMessages.Error.ToString();
                        }

                        return RepositoriesMessages.Error.ToString();
                    }

                    Context.Friendship.Add(new Friendship
                    {
                        requester_id = requesterId,
                        addressee_id = addresseeId,
                        status_id = pendingStatusId
                    });

                    Context.SaveChanges();
                    return RepositoriesMessages.Success.ToString();
                }

                return RepositoriesMessages.Error.ToString();
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public string AcceptFriendRequest(int requestId)
        {
            try
            {
                int pendingStatusId = GetPendingStatusId();
                int acceptedStatusId = GetAcceptedStatusId();

                if (pendingStatusId > 0 && acceptedStatusId > 0)
                {
                    var requestEntity = Context.Friendship.FirstOrDefault(req =>
                                        req.friendship_id == requestId && req.status_id == pendingStatusId);

                    if (requestEntity != null)
                    {
                        requestEntity.status_id = acceptedStatusId;
                        Context.SaveChanges();
                        return RepositoriesMessages.Success.ToString();
                    }
                }

                return RepositoriesMessages.Error.ToString();
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public string RejectFriendRequest(int requestId)
        {
            try
            {
                int pendingStatusId = GetPendingStatusId();

                if (pendingStatusId > 0)
                {
                    var requestEntity = Context.Friendship.FirstOrDefault(req =>
                    req.friendship_id == requestId && req.status_id == pendingStatusId);

                    if (requestEntity != null)
                    {
                        Context.Friendship.Remove(requestEntity);
                        Context.SaveChanges();
                        return RepositoriesMessages.Success.ToString();
                    }
                }

                return RepositoriesMessages.Error.ToString();
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public string RemoveFriendship(string addresseeUsername, string requesterUsername)
        {
            try
            {
                var addressee = playerRepository.GetPlayerByUsername(addresseeUsername);
                var requester = playerRepository.GetPlayerByUsername(requesterUsername);

                if (addressee == null || requester == null)
                {
                    return RepositoriesMessages.PlayerNotFound.ToString();
                }

                int acceptedStatusId = GetAcceptedStatusId();

                if (acceptedStatusId > 0)
                {
                    var friendshipEntity = Context.Friendship.FirstOrDefault(friendship =>
                    ((friendship.requester_id == requester.player_id && friendship.addressee_id == addressee.player_id) ||
                    (friendship.requester_id == addressee.player_id && friendship.addressee_id == requester.player_id)) &&
                    friendship.status_id == acceptedStatusId);

                    if (friendshipEntity != null)
                    {
                        Context.Friendship.Remove(friendshipEntity);
                        Context.SaveChanges();
                        return RepositoriesMessages.Success.ToString();
                    }
                }

                return RepositoriesMessages.Error.ToString();
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }
    }
}
