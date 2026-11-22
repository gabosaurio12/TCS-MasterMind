using MasterMind_Server.Contracts;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services;
using MasterMind_Server.Services.MatchHelpers;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System;
using System.Linq;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class MatchCallbackTests
    {
        private const int PointsToWinMatch = 2;

        private static readonly TimeSpan NoCallbackTimeout = TimeSpan.FromMilliseconds(800);

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
        public void TestValidateTrySolvedNotifiesOpponentFinished()
        {
            firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());

            Assert.That(secondProbe.WaitForOpponentFinished(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestValidateTrySolvedReportsSolvedProgress()
        {
            firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());
            secondProbe.WaitForOpponentFinished(TestDataFactory.CallbackTimeout);

            Assert.That(secondProbe.LastProgress.Solved, Is.True);
        }

        [Test]
        public void TestValidateTryWrongTryDoesNotNotifyOpponent()
        {
            firstClient.Service.ValidateTry(firstPlayer.username, BuildWrongTry());

            Assert.That(secondProbe.WaitForOpponentFinished(NoCallbackTimeout), Is.False);
        }

        [Test]
        public void TestValidateTryFirstFinisherDoesNotReceiveRoundResolved()
        {
            firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());

            Assert.That(firstProbe.WaitForRoundResolved(NoCallbackTimeout), Is.False);
        }

        [Test]
        public void TestValidateTrySecondFinisherReceivesRoundResolved()
        {
            firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());
            secondClient.Service.ValidateTry(secondPlayer.username, BuildCorrectTry());

            Assert.That(secondProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestValidateTryFewerTriesWinsRound()
        {
            firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());
            secondClient.Service.ValidateTry(secondPlayer.username, BuildWrongTry());
            secondClient.Service.ValidateTry(secondPlayer.username, BuildCorrectTry());
            firstProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout);

            Assert.That(firstProbe.LastResult.WinnerUsername, Is.EqualTo(firstPlayer.username));
        }

        [Test]
        public void TestValidateTrySameTriesIsTie()
        {
            firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());
            secondClient.Service.ValidateTry(secondPlayer.username, BuildCorrectTry());
            firstProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout);

            Assert.That(firstProbe.LastResult.WinnerUsername, Is.Null);
        }

        [Test]
        public void TestValidateTryOpponentNotSubscribedDoesNotThrow()
        {
            secondClient.Dispose();

            Assert.DoesNotThrow(
                () => firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry()));
        }

        [Test]
        public void TestUnsubscribeOpenRoundNotifiesOpponentAbandoned()
        {
            firstClient.Service.Unsubscribe(firstPlayer.username);

            Assert.That(secondProbe.WaitForOpponentAbandoned(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestUnsubscribeResolvedMatchDoesNotNotifyOpponent()
        {
            PlayRoundsWonByFirst(PointsToWinMatch);

            firstClient.Service.Unsubscribe(firstPlayer.username);

            Assert.That(secondProbe.WaitForOpponentAbandoned(NoCallbackTimeout), Is.False);
        }

        [Test]
        public void TestUnsubscribeAfterTieNotifiesOpponentAbandoned()
        {
            PlayTie();

            firstClient.Service.Unsubscribe(firstPlayer.username);

            Assert.That(secondProbe.WaitForOpponentAbandoned(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestValidateTryRoundWonAddsPointToWinner()
        {
            PlayRoundsWonByFirst(1);

            Assert.That(PointsOf(firstProbe.LastResult, firstPlayer.username), Is.EqualTo(1));
        }

        [Test]
        public void TestValidateTryRoundWonKeepsMatchOpen()
        {
            PlayRoundsWonByFirst(PointsToWinMatch - 1);

            Assert.That(firstProbe.LastResult.IsMatchOver, Is.False);
        }

        [Test]
        public void TestValidateTryRoundWonOpensNextRoundWithNewSecret()
        {
            PlayRoundsWonByFirst(1);

            Assert.DoesNotThrow(() => firstClient.Service.ValidateTry(firstPlayer.username, BuildWrongTry()));
        }

        [Test]
        public void TestValidateTryLastPointRoundWonEndsMatch()
        {
            PlayRoundsWonByFirst(PointsToWinMatch);

            Assert.That(firstProbe.LastResult.IsMatchOver, Is.True);
        }

        [Test]
        public void TestValidateTryTieDoesNotConsumeRound()
        {
            PlayTie();
            PlayTie();

            Assert.That(firstProbe.LastResult.RoundNumber, Is.EqualTo(1));
        }

        [Test]
        public void TestValidateTryTieAwardsNoPoint()
        {
            PlayTie();

            Assert.That(PointsOf(firstProbe.LastResult, firstPlayer.username), Is.EqualTo(0));
        }

        [Test]
        public void TestValidateTryOutOfTriesFirstThenOpponentSolvesResolvesRound()
        {
            int maxTries = TriesRules.GetMaxTries(TestDataFactory.DefaultDifficulty);
            for (int attempt = 0; attempt < maxTries; attempt++)
            {
                firstClient.Service.ValidateTry(firstPlayer.username, BuildWrongTry());
            }

            secondClient.Service.ValidateTry(secondPlayer.username, BuildCorrectTry());

            Assert.That(secondProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout), Is.True);
        }

        [Test]
        public void TestValidateTryOutOfTriesFirstThenOpponentSolvesOpponentWins()
        {
            int maxTries = TriesRules.GetMaxTries(TestDataFactory.DefaultDifficulty);
            for (int attempt = 0; attempt < maxTries; attempt++)
            {
                firstClient.Service.ValidateTry(firstPlayer.username, BuildWrongTry());
            }

            secondClient.Service.ValidateTry(secondPlayer.username, BuildCorrectTry());
            firstProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout);

            Assert.That(firstProbe.LastResult.WinnerUsername, Is.EqualTo(secondPlayer.username));
        }

        [Test]
        public void TestGetMaxTriesRoomWithoutTriesConfigReturnsDifficultyLimit()
        {
            int maxTries = firstClient.Service.GetMaxTries(firstPlayer.username);

            Assert.That(maxTries, Is.EqualTo(TriesRules.GetMaxTries(TestDataFactory.DefaultDifficulty)));
        }

        [Test]
        public void TestChannelClosedWithoutUnsubscribeDoesNotNotifyOpponentAbandoned()
        {
            firstClient.Dispose();

            Assert.That(secondProbe.WaitForOpponentAbandoned(NoCallbackTimeout), Is.False);
        }

        private void PlayRoundsWonByFirst(int rounds)
        {
            for (int round = 0; round < rounds; round++)
            {
                firstProbe.ResetRoundResolved();
                firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());
                secondClient.Service.ValidateTry(secondPlayer.username, BuildWrongTry());
                secondClient.Service.ValidateTry(secondPlayer.username, BuildCorrectTry());
                firstProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout);
            }
        }

        private void PlayTie()
        {
            firstProbe.ResetRoundResolved();
            firstClient.Service.ValidateTry(firstPlayer.username, BuildCorrectTry());
            secondClient.Service.ValidateTry(secondPlayer.username, BuildCorrectTry());
            firstProbe.WaitForRoundResolved(TestDataFactory.CallbackTimeout);
        }

        private static int PointsOf(RoundResult result, string username)
        {
            return result.Scores.Single(score => score.Username == username).Points;
        }

        private Room CreateReadyRoom()
        {
            var roomService = new RoomService();
            var newRoom = new Room
            {
                Name = TestDataFactory.CreateUniqueRoomName(),
                Gamemode = TestDataFactory.DefaultGamemode,
                Difficulty = TestDataFactory.DefaultDifficulty,
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
