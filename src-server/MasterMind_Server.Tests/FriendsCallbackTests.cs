using MasterMind_Server.Contracts;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class FriendsCallbackTests
    {
        private static readonly TimeSpan NoCallbackTimeout = TimeSpan.FromMilliseconds(800);

        private FriendsServiceHost host;
        private PlayerEntity requester;
        private PlayerEntity addressee;
        private FriendsCallbackProbe addresseeProbe;
        private FriendsCallbackProbe requesterProbe;
        private FriendsServiceClient addresseeClient;
        private FriendsServiceClient requesterClient;

        [SetUp]
        public void SetUp()
        {
            TestDatabase.CleanupTestData();
            requester = TestDataFactory.CreatePlayer();
            addressee = TestDataFactory.CreatePlayer();
            host = new FriendsServiceHost();
            host.Start();
            addresseeProbe = new FriendsCallbackProbe();
            requesterProbe = new FriendsCallbackProbe();
            addresseeClient = host.CreateClient(addresseeProbe);
            requesterClient = host.CreateClient(requesterProbe);
            addresseeClient.Service.Subscribe(addressee.username);
            requesterClient.Service.Subscribe(requester.username);
        }

        [TearDown]
        public void TearDown()
        {
            addresseeClient.Dispose();
            requesterClient.Dispose();
            host.Dispose();
            TestDatabase.CleanupTestData();
        }

        [Test]
        public void TestSendFriendRequestNotifiesAddressee()
        {
            requesterClient.Service.SendFriendRequest(addressee.player_id, requester.player_id);

            Assert.That(addresseeProbe.WaitForChange(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestSendFriendRequestDoesNotNotifySender()
        {
            requesterClient.Service.SendFriendRequest(addressee.player_id, requester.player_id);

            Assert.That(requesterProbe.WaitForChange(NoCallbackTimeout), Is.False);
        }

        [Test]
        public void TestAcceptFriendRequestNotifiesRequester()
        {
            requesterClient.Service.SendFriendRequest(addressee.player_id, requester.player_id);
            var request = requesterClient.Service.GetFriendRequest(addressee.player_id, requester.player_id);

            addresseeClient.Service.AcceptFriendRequest(request.FriendshipId);

            Assert.That(requesterProbe.WaitForChange(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestAcceptFriendRequestDoesNotNotifyAcceptor()
        {
            requesterClient.Service.SendFriendRequest(addressee.player_id, requester.player_id);
            var request = requesterClient.Service.GetFriendRequest(addressee.player_id, requester.player_id);
            addresseeProbe.Reset();

            addresseeClient.Service.AcceptFriendRequest(request.FriendshipId);

            Assert.That(addresseeProbe.WaitForChange(NoCallbackTimeout), Is.False);
        }

        [Test]
        public void TestRemoveFriendshipNotifiesOtherPlayer()
        {
            requesterClient.Service.SendFriendRequest(addressee.player_id, requester.player_id);
            var request = requesterClient.Service.GetFriendRequest(addressee.player_id, requester.player_id);
            addresseeClient.Service.AcceptFriendRequest(request.FriendshipId);
            addresseeProbe.Reset();

            requesterClient.Service.RemoveFriendship(addressee.username, requester.username);

            Assert.That(addresseeProbe.WaitForChange(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestRejectFriendRequestDoesNotNotify()
        {
            requesterClient.Service.SendFriendRequest(addressee.player_id, requester.player_id);
            var request = requesterClient.Service.GetFriendRequest(addressee.player_id, requester.player_id);
            requesterProbe.Reset();

            addresseeClient.Service.RejectFriendRequest(request.FriendshipId);

            Assert.That(requesterProbe.WaitForChange(NoCallbackTimeout), Is.False);
        }

        [Test]
        public void TestUnsubscribeStopsNotifications()
        {
            addresseeClient.Service.Unsubscribe(addressee.username);

            requesterClient.Service.SendFriendRequest(addressee.player_id, requester.player_id);

            Assert.That(addresseeProbe.WaitForChange(NoCallbackTimeout), Is.False);
        }

        [Test]
        public void TestSendFriendRequestClosedAddresseeChannelStillSucceeds()
        {
            addresseeClient.Dispose();

            string result = requesterClient.Service.SendFriendRequest(addressee.player_id, requester.player_id);

            Assert.That(result, Is.EqualTo("Success"));
        }
    }
}
