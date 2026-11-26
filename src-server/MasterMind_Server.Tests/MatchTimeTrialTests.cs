using MasterMind_Server.Contracts;
using MasterMind_Server.Data;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services;
using MasterMind_Server.Services.MatchHelpers;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System;
using System.Linq;
using System.ServiceModel;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class MatchTimeTrialTests
    {
        private const string TimeTrialGamemode = "TimeTrial";
        private const string EnigmaDifficulty = "Enigma";
        private const int EnigmaMilliseconds = 10000;

        private static readonly TimeSpan ExpiryTimeout = TimeSpan.FromSeconds(15);

        private MatchServiceHost host;
        private PlayerEntity firstPlayer;
        private PlayerEntity secondPlayer;
        private Room room;
        private MatchCallbackProbe firstProbe;
        private MatchCallbackProbe secondProbe;
        private MatchServiceClient firstClient;
        private MatchServiceClient secondClient;

        [SetUp]
        public void SetUp()
        {
            TestDatabase.CleanupTestData();
            firstPlayer = TestDataFactory.CreatePlayer();
            secondPlayer = TestDataFactory.CreatePlayer();
            room = CreateReadyRoom();
            host = new MatchServiceHost();
            host.Start();
            firstProbe = new MatchCallbackProbe();
            secondProbe = new MatchCallbackProbe();
            firstClient = host.CreateClient(firstProbe);
            secondClient = host.CreateClient(secondProbe);
            firstClient.Service.Subscribe(firstPlayer.username);
            secondClient.Service.Subscribe(secondPlayer.username);
            firstClient.Service.UpdateCode(firstPlayer.username);
        }

        [TearDown]
        public void TearDown()
        {
            firstClient.Dispose();
            secondClient.Dispose();
            host.Dispose();
            TestDatabase.CleanupTestData();
        }

        [Test]
        public void TestValidateTryBeforeClockStartsThrowsRoundNotStarted()
        {
            var fault = Assert.Throws<FaultException<ErrorMessage>>(
                () => firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry()));

            Assert.That(fault.Detail.Message, Is.EqualTo(ErrorMessages.RoundNotStarted.ToString()));
        }

        [Test]
        public void TestReadyForRoundOnlyOnePlayerDoesNotStartClock()
        {
            firstClient.Service.ReadyForRound(firstPlayer.username);

            Assert.That(firstProbe.WaitForTimeTrialStarted(TimeSpan.FromMilliseconds(800)), Is.False);
        }

        [Test]
        public void TestReadyForRoundBothPlayersNotifiesStartToFirstPlayer()
        {
            StartClock();

            Assert.That(firstProbe.WaitForTimeTrialStarted(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestReadyForRoundBothPlayersNotifiesStartToSecondPlayer()
        {
            StartClock();

            Assert.That(secondProbe.WaitForTimeTrialStarted(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestReadyForRoundReportsRemainingTimeWithinDuration()
        {
            StartClock();
            secondProbe.WaitForTimeTrialStarted(TestDataFactory.CallbackTimeout);

            Assert.That(secondProbe.LastRemainingMs, Is.InRange(1, EnigmaMilliseconds));
        }

        [Test]
        public void TestReadyForRoundPersistsDeadline()
        {
            StartClock();

            Assert.That(GetPersistedRecord().DeadlineUtc, Is.Not.Null);
        }

        [Test]
        public void TestValidateTryFirstSolverResolvesRoundForOpponent()
        {
            StartClock();
            firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());

            Assert.That(secondProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestValidateTryFirstSolverWinsRound()
        {
            StartClock();
            firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());
            secondProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout);

            Assert.That(secondProbe.LastResult.WinnerUsername, Is.EqualTo(firstPlayer.username));
        }

        [Test]
        public void TestValidateTryWrongTryAllowsUnlimitedTries()
        {
            StartClock();
            for (int attempt = 0; attempt < 12; attempt++)
            {
                firstClient.Service.ValidateTry(firstPlayer.username, BuildWrongTry());
            }

            Assert.That(firstClient.Service.ValidateTry(firstPlayer.username, BuildWrongTry()).IsCorrect, Is.False);
        }

        [Test]
        public void TestRoundExpiresWithoutWinnerWhenNobodySolves()
        {
            StartClock();
            firstProbe.WaitForRoundResolved(ExpiryTimeout);

            Assert.That(firstProbe.LastResult.WinnerUsername, Is.Null);
        }

        [Test]
        public void TestRestoreAllExpiredDeadlineResolvesRoundAsTie()
        {
            StartClock();
            var record = GetPersistedRecord();
            record.DeadlineUtc = DateTime.UtcNow.AddSeconds(-1);
            new MatchRoundStateRepository().Save(record);
            new MatchRestorer().RestoreAll();
            firstProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout);

            Assert.That(firstProbe.LastResult.WinnerUsername, Is.Null);
        }

        [Test]
        public void TestGetMatchSnapshotAfterClockStartReportsRunning()
        {
            StartClock();

            Assert.That(firstClient.Service.GetMatchSnapshot(firstPlayer.username).IsClockRunning, Is.True);
        }

        private void StartClock()
        {
            firstClient.Service.ReadyForRound(firstPlayer.username);
            secondClient.Service.ReadyForRound(secondPlayer.username);
        }

        private MatchRoundRecord GetPersistedRecord()
        {
            return new MatchRoundStateRepository().GetAll().Single(record => record.RoomId == room.RoomId);
        }

        private Room CreateReadyRoom()
        {
            var roomService = new RoomService();
            var newRoom = new Room
            {
                Name = TestDataFactory.CreateUniqueRoomName(),
                Gamemode = TimeTrialGamemode,
                Difficulty = EnigmaDifficulty,
                PlayerOneUsername = firstPlayer.username,
                RoomPrivacy = TestDataFactory.PublicPrivacy
            };
            var created = roomService.CreateRoom(newRoom).Room;
            roomService.JoinRoom(newRoom, secondPlayer.username);
            SetReady(created.RoomId, firstPlayer.username, true);
            SetReady(created.RoomId, secondPlayer.username, false);
            return created;
        }

        private static void SetReady(int roomId, string username, bool isPlayerOne)
        {
            new LobbyService().ChangePlayerStatus(new PlayerStatus
            {
                RoomId = roomId,
                Username = username,
                Status = PlayerPossibleStatus.Ready.ToString(),
                IsPlayerOne = isPlayerOne
            });
        }

        private CodeTry BuildCorrectTry()
        {
            return new CodeTry { Code = TestDatabase.GetSecretCode(room.RoomId) };
        }

        private CodeTry BuildWrongTry()
        {
            var code = TestDatabase.GetSecretCode(room.RoomId);
            code[0] = code[0] == Colours.Blue.ToString() ? Colours.Red.ToString() : Colours.Blue.ToString();
            return new CodeTry { Code = code };
        }
    }
}
