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
        private readonly static MasterMindEntities Context = new MasterMindEntities();

        private readonly static ILog logger = LogManager.GetLogger(typeof(FriendshipService));
        private readonly static int PendantStatusId = Context.RequestStatusCatalog.FirstOrDefault(rq => rq.status == RequestStatusEnum.Pendant.ToString()).request_status_id;
        private readonly static int AcceptedStatusId = Context.RequestStatusCatalog.FirstOrDefault(rq => rq.status == RequestStatusEnum.Accepted.ToString()).request_status_id;

        public static RequestResult SendFrienshipRequest(int requesterId, int addresseeId)
        {
            try
            {
                var request = Context.Friendship.FirstOrDefault(
                    r => r.requester_id == requesterId &&
                    r.addressee_id == addresseeId &&
                    r.status_id == PendantStatusId);

                if (request == null)
                {
                    Context.Friendship.Add(
                        new Friendship
                        {
                            requester_id = requesterId,
                            addressee_id = addresseeId,
                            status_id = PendantStatusId
                        });

                    Context.SaveChanges();

                    return RequestResult.Success;
                }
                else
                {
                    if (request.status_id == PendantStatusId)
                        return RequestResult.RequestIsPendant;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            
            return RequestResult.Error;
        }

        public static RequestResult AcceptFriendRequest(int requestId)
        {
            try
            {
                var request = Context.Friendship.FirstOrDefault(
                r => r.friendship_id == requestId &&
                r.status_id == PendantStatusId);

                if (request != null)
                {
                    request.status_id = AcceptedStatusId;
                    Context.SaveChanges();
                    return RequestResult.Success;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return RequestResult.Error;
        }

        public static RequestResult RejectFriendRequest(int requestId)
        {
            try
            {
                var request = Context.Friendship.FirstOrDefault(
                r => r.friendship_id == requestId &&
                r.status_id == PendantStatusId);

                if (request != null)
                {
                    Context.Friendship.Remove(request);
                    Context.SaveChanges();
                    return RequestResult.Success;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            
            return RequestResult.Error;
        }

        public static List<Player> GetFriendRequests(int playerId)
        {
            try
            {
                var friendships = Context.Friendship.Where(f => f.addressee_id == playerId &&
                f.status_id == PendantStatusId).ToList();

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
                var friendships = Context.Friendship.Where(f => (
                f.requester_id == playerId ||
                f.addressee_id == playerId) &&
                f.status_id == AcceptedStatusId).ToList();

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
                var request = Context.Friendship.FirstOrDefault(
                r => r.requester_id == requesterId &&
                r.addressee_id == addresseeId &&
                r.status_id == PendantStatusId);

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
                var request = Context.Friendship.FirstOrDefault(
                                r => r.requester_id == requesterId &&
                                r.addressee_id == addresseeId &&
                                r.status_id == AcceptedStatusId);

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
