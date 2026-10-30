using MasterMind_Server.Contracts;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System.Collections.Generic;
using PlayerContract = MasterMind_Server.Contracts.Player;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class FriendsServiceTests
    {
        private FriendsService friendsService;
        private PlayerEntity requester;
        private PlayerEntity addressee;

        [SetUp]
        public void SetUp()
        {
            TestDatabase.CleanupTestData();
            friendsService = new FriendsService();
            requester = TestDataFactory.CreatePlayer();
            addressee = TestDataFactory.CreatePlayer();
        }

        [TearDown]
        public void TearDown()
        {
            TestDatabase.CleanupTestData();
        }

        [Test]
        public void TestSendFriendRequestSuccess()
        {
            string result = friendsService.SendFriendRequest(addressee.player_id, requester.player_id);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Success.ToString()));
        }

        [Test]
        public void TestSendFriendRequestDuplicateReturnsPending()
        {
            friendsService.SendFriendRequest(addressee.player_id, requester.player_id);

            string result = friendsService.SendFriendRequest(addressee.player_id, requester.player_id);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Pending.ToString()));
        }

        [Test]
        public void TestSendFriendRequestToSelfReturnsError()
        {
            string result = friendsService.SendFriendRequest(requester.player_id, requester.player_id);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Error.ToString()));
        }

        [Test]
        public void TestGetFriendRequestSuccessReturnsPendingRequest()
        {
            friendsService.SendFriendRequest(addressee.player_id, requester.player_id);

            Friendship result = friendsService.GetFriendRequest(addressee.player_id, requester.player_id);

            Assert.That(result.Status, Is.EqualTo(FriendshipStatusEnum.Pending.ToString()));
        }

        [Test]
        public void TestGetFriendRequestNotFoundReturnsEmptyFriendship()
        {
            Friendship result = friendsService.GetFriendRequest(addressee.player_id, requester.player_id);

            Assert.That(result.FriendshipId, Is.EqualTo(0));
        }

        [Test]
        public void TestAcceptFriendRequestSuccess()
        {
            friendsService.SendFriendRequest(addressee.player_id, requester.player_id);
            Friendship request = friendsService.GetFriendRequest(addressee.player_id, requester.player_id);

            string result = friendsService.AcceptFriendRequest(request.FriendshipId);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Success.ToString()));
        }

        [Test]
        public void TestAcceptUnknownFriendRequestReturnsError()
        {
            string result = friendsService.AcceptFriendRequest(int.MaxValue);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Error.ToString()));
        }

        [Test]
        public void TestRejectFriendRequestSuccess()
        {
            friendsService.SendFriendRequest(addressee.player_id, requester.player_id);
            Friendship request = friendsService.GetFriendRequest(addressee.player_id, requester.player_id);

            string result = friendsService.RejectFriendRequest(request.FriendshipId);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Success.ToString()));
        }

        [Test]
        public void TestRejectUnknownFriendRequestReturnsError()
        {
            string result = friendsService.RejectFriendRequest(int.MaxValue);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Error.ToString()));
        }

        [Test]
        public void TestGetFriendRequestsReturnsPendingRequester()
        {
            friendsService.SendFriendRequest(addressee.player_id, requester.player_id);

            List<PlayerContract> result = friendsService.GetFriendRequests(addressee.player_id);

            Assert.That(result[0].Username, Is.EqualTo(requester.username));
        }

        [Test]
        public void TestGetFriendRequestsWithoutRequestsReturnsEmptyList()
        {
            List<PlayerContract> result = friendsService.GetFriendRequests(addressee.player_id);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void TestGetFriendshipsAfterAcceptReturnsFriend()
        {
            friendsService.SendFriendRequest(addressee.player_id, requester.player_id);
            Friendship request = friendsService.GetFriendRequest(addressee.player_id, requester.player_id);
            friendsService.AcceptFriendRequest(request.FriendshipId);

            List<PlayerContract> result = friendsService.GetFriendships(requester.player_id);

            Assert.That(result[0].Username, Is.EqualTo(addressee.username));
        }

        [Test]
        public void TestGetFriendshipsWithoutFriendsReturnsEmptyList()
        {
            List<PlayerContract> result = friendsService.GetFriendships(requester.player_id);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void TestGetFriendshipAfterAcceptReturnsAcceptedStatus()
        {
            friendsService.SendFriendRequest(addressee.player_id, requester.player_id);
            Friendship request = friendsService.GetFriendRequest(addressee.player_id, requester.player_id);
            friendsService.AcceptFriendRequest(request.FriendshipId);

            Friendship result = friendsService.GetFriendship(addressee.player_id, requester.player_id);

            Assert.That(result.Status, Is.EqualTo(FriendshipStatusEnum.Accepted.ToString()));
        }

        [Test]
        public void TestGetPendingFriendshipReturnsEmptyFriendship()
        {
            friendsService.SendFriendRequest(addressee.player_id, requester.player_id);

            Friendship result = friendsService.GetFriendship(addressee.player_id, requester.player_id);

            Assert.That(result.FriendshipId, Is.EqualTo(0));
        }

        [Test]
        public void TestRemoveFriendshipSuccess()
        {
            friendsService.SendFriendRequest(addressee.player_id, requester.player_id);
            Friendship request = friendsService.GetFriendRequest(addressee.player_id, requester.player_id);
            friendsService.AcceptFriendRequest(request.FriendshipId);

            string result = friendsService.RemoveFriendship(addressee.username, requester.username);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Success.ToString()));
        }

        [Test]
        public void TestRemoveFriendshipUnknownUserReturnsPlayerNotFound()
        {
            string result = friendsService.RemoveFriendship(
                TestDataFactory.CreateUniqueUsername(),
                requester.username);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.PlayerNotFound.ToString()));
        }

        [Test]
        public void TestRemoveFriendshipWithoutAcceptedFriendshipReturnsError()
        {
            string result = friendsService.RemoveFriendship(addressee.username, requester.username);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Error.ToString()));
        }
    }
}
