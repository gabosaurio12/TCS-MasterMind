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
                        .FirstOrDefault(rq => rq.status == RequestStatusEnum.RequestIsPendant.ToString()).request_status_id;

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
                    r => r.requester_id == requesterId &&
                    r.addressee_id == addresseeId &&
                    r.status_id == pendingStatusId);

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
                        return RequestStatusEnum.RequestIsPendant;
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
                r => r.friendship_id == requestId &&
                r.status_id == pendingStatusId);

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
                r => r.friendship_id == requestId &&
                r.status_id == pendingStatusId);

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

                var friendships = Context.Friendship.Where(f => f.addressee_id == playerId &&
                f.status_id == pendingStatusId).ToList();

                List<int> friendsIds = new List<int>();

                foreach (var i in friendships)
                {
                    friendsIds.Add(i.addressee_id != playerId ? i.addressee_id : i.requester_id);
                }

                List<Player> friends = new List<Player>();

                foreach (var i in friendsIds)
                {
                    var friend = Context.Player.FirstOrDefault(p => p.player_id == i);
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

        public static List<Player> GetFrienships(int playerId)
        {
            try
            {
                EnsureCatalogsLoaded();

                var friendships = Context.Friendship.Where(f => (
                f.requester_id == playerId ||
                f.addressee_id == playerId) &&
                f.status_id == acceptedStatusId).ToList();

                List<int> friendsIds = new List<int>();

                foreach (var i in friendships)
                {
                    friendsIds.Add(i.addressee_id != playerId ? i.addressee_id : i.requester_id);
                }

                List<Player> friends = new List<Player>();

                foreach (var i in friendsIds)
                {
                    var friend = Context.Player.FirstOrDefault(p => p.player_id == i);
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
                r => r.requester_id == requesterId &&
                r.addressee_id == addresseeId &&
                r.status_id == pendingStatusId);

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
                                r => r.requester_id == requesterId &&
                                r.addressee_id == addresseeId &&
                                r.status_id == acceptedStatusId);

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
