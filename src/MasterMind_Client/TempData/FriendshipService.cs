using log4net;
using MasterMind_Client.Data;
using MasterMind_Client.TempData.Enum;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;

namespace MasterMind_Client.TempData
{
    public static class FriendshipService
    {
        private readonly static MasterMindEntities Context = new MasterMindEntities(true);

        private readonly static ILog logger = LogManager.GetLogger(typeof(FriendshipService));
        private static int pendingStatusId;
        private static int acceptedStatusId;
        private static readonly object catalogLock = new object();

        private static void EnsureCatalogsLoaded()
        {
            if (pendingStatusId != 0)
            {
                return;
            }

            lock (catalogLock)
            {
                if (pendingStatusId != 0)
                {
                    return;
                }

                using (var context = new MasterMindEntities(true))
                {
                    pendingStatusId = context.RequestStatusCatalog
                        .FirstOrDefault(rq => rq.status == RequestStatusEnum.Pending.ToString()).request_status_id;

                    acceptedStatusId = context.RequestStatusCatalog
                        .FirstOrDefault(rq => rq.status == RequestStatusEnum.Accepted.ToString()).request_status_id;
                }
            }
        }

        public static RequestStatusEnum SendFrienshipRequest(int requesterId, int addresseeId)
        {
            try
            {
                EnsureCatalogsLoaded();

                var request = Context.Friendship.FirstOrDefault(
                    req => req.requester_id == requesterId &&
                    req.addressee_id == addresseeId &&
                    req.status_id == pendingStatusId);

                if (request == null)
                {
                    Context.Friendship.Add(
                        new Friendship
                        {
                            requester_id = requesterId,
                            addressee_id = addresseeId,
                            status_id = pendingStatusId
                        });

                    Context.SaveChanges();

                    return RequestStatusEnum.Success;
                }
                else
                {
                    if (request.status_id == pendingStatusId)
                        return RequestStatusEnum.Pending;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            
            return RequestStatusEnum.Error;
        }

        public static RequestStatusEnum AcceptFriendRequest(int requestId)
        {
            try
            {
                EnsureCatalogsLoaded();

                var request = Context.Friendship.FirstOrDefault(
                req => req.friendship_id == requestId &&
                req.status_id == pendingStatusId);

                if (request != null)
                {
                    request.status_id = acceptedStatusId;
                    Context.SaveChanges();
                    return RequestStatusEnum.Success;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return RequestStatusEnum.Error;
        }

        public static RequestStatusEnum RejectFriendRequest(int requestId)
        {
            try
            {
                EnsureCatalogsLoaded();

                var request = Context.Friendship.FirstOrDefault(
                req => req.friendship_id == requestId &&
                req.status_id == pendingStatusId);

                if (request != null)
                {
                    Context.Friendship.Remove(request);
                    Context.SaveChanges();
                    return RequestStatusEnum.Success;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            
            return RequestStatusEnum.Error;
        }

        public static List<Player> GetFriendRequests(int playerId)
        {
            try
            {
                EnsureCatalogsLoaded();

                var friendships = Context.Friendship.Where(friendship => friendship.addressee_id == playerId
                    && friendship.status_id == pendingStatusId).ToList();

                List<int> friendsIds = GetFriendsIds(friendships, playerId);

                List<Player> friends = new List<Player>();

                foreach (var friendId in friendsIds)
                {
                    var friend = Context.Player.FirstOrDefault(player => player.player_id == friendId);
                    if (friend != null)
                    {
                        friends.Add(friend);
                    }
                }

                return friends;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return new List<Player>();
            
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

        public static List<Player> GetFrienships(int playerId)
        {
            try
            {
                EnsureCatalogsLoaded();

                var friendships = Context.Friendship.Where(friendship => (
                    friendship.requester_id == playerId ||
                    friendship.addressee_id == playerId) &&
                    friendship.status_id == acceptedStatusId).ToList();

                List<int> friendsIds = GetFriendsIds(friendships, playerId);

                List<Player> friends = new List<Player>();

                foreach (var friendId in friendsIds)
                {
                    var friend = TempPlayerService.GetPlayerById(friendId);
                    if (friend != null)
                    {
                        friends.Add(friend);
                    }
                }

                return friends;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return new List<Player>();            
        }

        public static Friendship GetFriendRequest(int requesterId, int addresseeId)
        {
            try
            {
                EnsureCatalogsLoaded();

                var request = Context.Friendship.FirstOrDefault(
                    req => req.requester_id == requesterId &&
                    req.addressee_id == addresseeId &&
                    req.status_id == pendingStatusId);

                if (request != null)
                {
                    return request;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return null;
            
        }

        public static Friendship GetFriendship(int requesterId, int addresseeId)
        {
            try
            {
                EnsureCatalogsLoaded();

                var request = Context.Friendship.FirstOrDefault(
                                req => req.requester_id == requesterId &&
                                req.addressee_id == addresseeId &&
                                req.status_id == acceptedStatusId);

                if (request != null)
                {
                    return request;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            
            return null;
        }
    }
}
