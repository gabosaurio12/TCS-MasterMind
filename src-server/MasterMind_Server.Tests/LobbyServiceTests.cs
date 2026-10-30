using MasterMind_Server.Contracts;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System;
using System.ServiceModel;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class LobbyServiceTests
    {
        private LobbyServiceHost lobbyHost;
        private LobbyServiceClient firstClient;
        private LobbyServiceClient secondClient;
        private LobbyCallbackProbe firstProbe;
        private LobbyCallbackProbe secondProbe;
        private Room room;
        private PlayerEntity firstPlayer;
        private PlayerEntity secondPlayer;

        [SetUp]
        public void SetUp()
        {
            TestDatabase.CleanupTestData();
            lobbyHost = new LobbyServiceHost();
            lobbyHost.Start();
            firstPlayer = TestDataFactory.CreatePlayer();
            secondPlayer = TestDataFactory.CreatePlayer();
            room = CreatePublicRoom(firstPlayer, secondPlayer);
            firstProbe = new LobbyCallbackProbe();
            secondProbe = new LobbyCallbackProbe();
            firstClient = lobbyHost.CreateClient(firstProbe);
            secondClient = lobbyHost.CreateClient(secondProbe);
        }

        [TearDown]
        public void TearDown()
        {
            DisposeClient(firstClient);
            DisposeClient(secondClient);
            lobbyHost.Dispose();
            TestDatabase.CleanupTestData();
        }

        [Test]
        public void TestSendMessageSuccess()
        {
            string result = firstClient.Service.SendMessage(BuildMessage("hello"));

            Assert.That(result, Is.EqualTo(LobbyResultEnum.MessageSent.ToString()));
        }

        [Test]
        public void TestSendMessageNotifiesOtherClient()
        {
            RegisterBothClients();

            firstClient.Service.SendMessage(BuildMessage("hello"));
            bool notified = secondProbe.WaitForMessage(TestDataFactory.CallbackTimeout);

            Assert.That(notified, Is.True);
        }

        [Test]
        public void TestSendMessageDeliversMessageContent()
        {
            RegisterBothClients();

            firstClient.Service.SendMessage(BuildMessage("hello"));
            secondProbe.WaitForMessage(TestDataFactory.CallbackTimeout);

            Assert.That(secondProbe.LastReceivedMessage.Message, Is.EqualTo("hello"));
        }

        [Test]
        public void TestSendMessageWithBannedWordReturnsInappropriateVocabulary()
        {
            string result = firstClient.Service.SendMessage(BuildMessage("mierda"));

            Assert.That(result, Is.EqualTo(LobbyResultEnum.InappropriateVocabulary.ToString()));
        }

        [Test]
        public void TestSendMessageWithBannedWordCreatesReport()
        {
            firstClient.Service.SendMessage(BuildMessage("mierda"));

            Assert.That(TestDatabase.DoesReportExist(firstPlayer.username), Is.True);
        }

        [Test]
        public void TestSendMessageWithEmptyTextReturnsError()
        {
            string result = firstClient.Service.SendMessage(BuildMessage(string.Empty));

            Assert.That(result, Is.EqualTo(LobbyResultEnum.Error.ToString()));
        }

        [Test]
        public void TestSendMessageWithTooLongTextReturnsError()
        {
            string result = firstClient.Service.SendMessage(BuildMessage(new string('a', 201)));

            Assert.That(result, Is.EqualTo(LobbyResultEnum.Error.ToString()));
        }

        [Test]
        public void TestSendMessageFromNonMemberReturnsError()
        {
            ChatMessage message = BuildMessage("hello");
            message.SenderUsername = TestDataFactory.CreateUniqueUsername();

            string result = firstClient.Service.SendMessage(message);

            Assert.That(result, Is.EqualTo(LobbyResultEnum.Error.ToString()));
        }

        [Test]
        public void TestSendMessageToUnknownRoomReturnsError()
        {
            ChatMessage message = BuildMessage("hello");
            message.RoomId = int.MaxValue;

            string result = firstClient.Service.SendMessage(message);

            Assert.That(result, Is.EqualTo(LobbyResultEnum.Error.ToString()));
        }

        [Test]
        public void TestChangePlayerStatusSuccess()
        {
            string result = firstClient.Service.ChangePlayerStatus(
                BuildStatus(firstPlayer.username, PlayerStatusEnum.Ready));

            Assert.That(result, Is.EqualTo(RoomResultEnum.Success.ToString()));
        }

        [Test]
        public void TestChangePlayerStatusInvalidStatusReturnsError()
        {
            PlayerStatus status = BuildStatus(firstPlayer.username, PlayerStatusEnum.Ready);
            status.Status = "UnknownStatus";

            string result = firstClient.Service.ChangePlayerStatus(status);

            Assert.That(result, Is.EqualTo(RoomResultEnum.Error.ToString()));
        }

        [Test]
        public void TestChangePlayerStatusNonMemberReturnsError()
        {
            string result = firstClient.Service.ChangePlayerStatus(
                BuildStatus(TestDataFactory.CreateUniqueUsername(), PlayerStatusEnum.Ready));

            Assert.That(result, Is.EqualTo(RoomResultEnum.Error.ToString()));
        }

        [Test]
        public void TestChangePlayerStatusNotifiesOtherClient()
        {
            firstClient.Service.ChangePlayerStatus(BuildStatus(firstPlayer.username, PlayerStatusEnum.Ready));

            secondClient.Service.ChangePlayerStatus(BuildStatus(secondPlayer.username, PlayerStatusEnum.Ready));
            bool notified = firstProbe.WaitForStatusChange(TestDataFactory.CallbackTimeout);

            Assert.That(notified, Is.True);
        }

        [Test]
        public void TestChangePlayerStatusDeliversChangedPlayer()
        {
            firstClient.Service.ChangePlayerStatus(BuildStatus(firstPlayer.username, PlayerStatusEnum.Ready));

            secondClient.Service.ChangePlayerStatus(BuildStatus(secondPlayer.username, PlayerStatusEnum.Ready));
            firstProbe.WaitForStatusChange(TestDataFactory.CallbackTimeout);

            Assert.That(firstProbe.LastStatusChange.Username, Is.EqualTo(secondPlayer.username));
        }

        [Test]
        public void TestEscapePlayerTwoClearsPlayerTwo()
        {
            secondClient.Service.ChangePlayerStatus(
                BuildStatus(secondPlayer.username, PlayerStatusEnum.Escaped));

            int? result = TestDatabase.GetRoomPlayerTwoId(room.RoomId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestEscapePlayerOnePromotesPlayerTwo()
        {
            firstClient.Service.ChangePlayerStatus(
                BuildStatus(firstPlayer.username, PlayerStatusEnum.Escaped));

            int? result = TestDatabase.GetRoomPlayerOneId(room.RoomId);

            Assert.That(result, Is.EqualTo(secondPlayer.player_id));
        }

        [Test]
        public void TestEscapeSoloPlayerDeletesRoom()
        {
            Room soloRoom = CreatePublicRoom(firstPlayer);
            firstClient.Service.ChangePlayerStatus(
                BuildStatus(firstPlayer.username, PlayerStatusEnum.Escaped, soloRoom.RoomId));

            Room result = new RoomService().GetRoomByName(soloRoom.Name);

            Assert.That(result.RoomId, Is.EqualTo(0));
        }

        private Room CreatePublicRoom(params PlayerEntity[] players)
        {
            RoomService roomService = new RoomService();
            Room newRoom = new Room
            {
                Name = TestDataFactory.CreateUniqueRoomName(),
                Gamemode = TestDataFactory.DefaultGamemode,
                Difficulty = TestDataFactory.DefaultDifficulty,
                PlayerOneUsername = players[0].username,
                RoomPrivacy = TestDataFactory.PublicPrivacy
            };
            RoomCreationResult createdRoom = roomService.CreateRoom(newRoom);

            for (int index = 1; index < players.Length; index++)
            {
                roomService.JoinRoom(newRoom, players[index].username);
            }

            return createdRoom.Room;
        }

        private ChatMessage BuildMessage(string text)
        {
            return BuildMessage(text, firstPlayer.username);
        }

        private ChatMessage BuildMessage(string text, string senderUsername)
        {
            return new ChatMessage
            {
                RoomId = room.RoomId,
                SenderUsername = senderUsername,
                Message = text
            };
        }

        private PlayerStatus BuildStatus(string username, PlayerStatusEnum status, int? roomId = null)
        {
            return new PlayerStatus
            {
                RoomId = roomId ?? room.RoomId,
                Username = username,
                Status = status.ToString()
            };
        }

        private void RegisterBothClients()
        {
            firstClient.Service.SendMessage(BuildMessage("first"));
            secondClient.Service.SendMessage(BuildMessage("second", secondPlayer.username));
            firstProbe.Reset();
            secondProbe.Reset();
        }

        private static void DisposeClient(LobbyServiceClient client)
        {
            if (client != null)
            {
                client.Dispose();
            }
        }
    }
}
