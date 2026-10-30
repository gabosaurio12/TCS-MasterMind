using MasterMind_Server.Contracts;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System;
using System.ServiceModel;
using PlayerContract = MasterMind_Server.Contracts.Player;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class PlayerServiceTests
    {
        private PlayerService playerService;

        [SetUp]
        public void SetUp()
        {
            TestDatabase.CleanupTestData();
            playerService = new PlayerService();
        }

        [TearDown]
        public void TearDown()
        {
            TestDatabase.CleanupTestData();
        }

        [Test]
        public void TestRegisterPlayerSuccess()
        {
            PlayerContract player = TestDataFactory.CreateContractPlayer();

            string result = playerService.RegisterPlayer(player);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Success.ToString()));
        }

        [Test]
        public void TestRegisterPlayerCreatesVerificationCode()
        {
            PlayerContract player = TestDataFactory.CreateContractPlayer();
            playerService.RegisterPlayer(player);

            PlayerEntity registeredPlayer = TestDatabase.GetPlayerByUsername(player.Username);

            Assert.That(TestDatabase.DoesVerificationCodeExist(registeredPlayer.player_id), Is.True);
        }

        [Test]
        public void TestRegisterPlayerDuplicateUsernameReturnsUsernameTaken()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();
            PlayerContract duplicate = TestDataFactory.CreateContractPlayer();
            duplicate.Username = existingPlayer.username;

            string result = playerService.RegisterPlayer(duplicate);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.UsernameTaken.ToString()));
        }

        [Test]
        public void TestRegisterPlayerDuplicateEmailReturnsEmailTaken()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();
            PlayerContract duplicate = TestDataFactory.CreateContractPlayer();
            duplicate.Email = existingPlayer.email;

            string result = playerService.RegisterPlayer(duplicate);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.EmailTaken.ToString()));
        }

        [Test]
        public void TestGetPlayerByUsernameSuccess()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();

            PlayerContract result = playerService.GetPlayerByUsername(existingPlayer.username);

            Assert.That(result.PlayerId, Is.EqualTo(existingPlayer.player_id));
        }

        [Test]
        public void TestGetPlayerByUsernameNotFoundReturnsEmptyPlayer()
        {
            PlayerContract result = playerService.GetPlayerByUsername(TestDataFactory.CreateUniqueUsername());

            Assert.That(result.PlayerId, Is.EqualTo(TestDataFactory.InvalidPlayerId));
        }

        [Test]
        public void TestGetPlayerByIdSuccess()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();

            PlayerContract result = playerService.GetPlayerById(existingPlayer.player_id);

            Assert.That(result.Username, Is.EqualTo(existingPlayer.username));
        }

        [Test]
        public void TestGetPlayerByIdNotFoundReturnsEmptyPlayer()
        {
            PlayerContract result = playerService.GetPlayerById(TestDataFactory.InvalidPlayerId);

            Assert.That(result.PlayerId, Is.EqualTo(TestDataFactory.InvalidPlayerId));
        }

        [Test]
        public void TestLoginPlayerSuccess()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();

            bool result = playerService.LoginPlayer(existingPlayer.username, TestDataFactory.DefaultPassword);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestLoginPlayerWrongPasswordReturnsFalse()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();

            bool result = playerService.LoginPlayer(existingPlayer.username, "WrongPassword123!");

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestLoginPlayerUnknownUserReturnsFalse()
        {
            bool result = playerService.LoginPlayer(
                TestDataFactory.CreateUniqueUsername(),
                TestDataFactory.DefaultPassword);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestLoginPlayerMalformedHashThrowsFault()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();
            using (var context = new MasterMindEntities(true))
            {
                PlayerEntity player = context.Player.Find(existingPlayer.player_id);
                player.password = "invalid-hash";
                context.SaveChanges();
            }

            Assert.Throws<FaultException>(
                () => playerService.LoginPlayer(existingPlayer.username, TestDataFactory.DefaultPassword));
        }

        [Test]
        public void TestUpdatePlayerSuccess()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();
            PlayerContract update = new PlayerContract
            {
                PlayerId = existingPlayer.player_id,
                Username = TestDataFactory.CreateUniqueUsername(),
                Email = TestDataFactory.CreateUniqueEmail()
            };

            string result = playerService.UpdatePlayer(update);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Success.ToString()));
        }

        [Test]
        public void TestUpdatePlayerNotFoundReturnsPlayerNotFound()
        {
            PlayerContract update = TestDataFactory.CreateContractPlayer();
            update.PlayerId = int.MaxValue;

            string result = playerService.UpdatePlayer(update);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.PlayerNotFound.ToString()));
        }

        [Test]
        public void TestUpdatePlayerDuplicateUsernameReturnsUsernameTaken()
        {
            PlayerEntity firstPlayer = TestDataFactory.CreatePlayer();
            PlayerEntity secondPlayer = TestDataFactory.CreatePlayer();
            PlayerContract update = new PlayerContract
            {
                PlayerId = secondPlayer.player_id,
                Username = firstPlayer.username,
                Email = TestDataFactory.CreateUniqueEmail()
            };

            string result = playerService.UpdatePlayer(update);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.UsernameTaken.ToString()));
        }

        [Test]
        public void TestUpdatePlayerDuplicateEmailReturnsEmailTaken()
        {
            PlayerEntity firstPlayer = TestDataFactory.CreatePlayer();
            PlayerEntity secondPlayer = TestDataFactory.CreatePlayer();
            PlayerContract update = new PlayerContract
            {
                PlayerId = secondPlayer.player_id,
                Username = TestDataFactory.CreateUniqueUsername(),
                Email = firstPlayer.email
            };

            string result = playerService.UpdatePlayer(update);

            Assert.That(result, Is.EqualTo(RepositoriesEnum.EmailTaken.ToString()));
        }

        [Test]
        public void TestChangePasswordSuccess()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();

            string result = playerService.ChangePassword(existingPlayer.username, "NewPassword123!");

            Assert.That(result, Is.EqualTo(RepositoriesEnum.Success.ToString()));
        }

        [Test]
        public void TestChangePasswordUnknownUserReturnsPlayerNotFound()
        {
            string result = playerService.ChangePassword(
                TestDataFactory.CreateUniqueUsername(),
                "NewPassword123!");

            Assert.That(result, Is.EqualTo(RepositoriesEnum.PlayerNotFound.ToString()));
        }

        [Test]
        public void TestGetPlayerRecordsUnknownUserReturnsEmptyUsername()
        {
            PlayerRecords result = playerService.GetPlayerRecords(TestDataFactory.CreateUniqueUsername());

            Assert.That(result.PlayerUsername, Is.Empty);
        }

        [Test]
        public void TestGetPlayerRecordsSuccessReturnsUsername()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();

            PlayerRecords result = playerService.GetPlayerRecords(existingPlayer.username);

            Assert.That(result.PlayerUsername, Is.EqualTo(existingPlayer.username));
        }

        [Test]
        public void TestReportPlayerCreatesReport()
        {
            PlayerEntity reportedPlayer = TestDataFactory.CreatePlayer();
            PlayerEntity reporter = TestDataFactory.CreatePlayer();
            Report report = new Report
            {
                ReportedPlayerUsername = reportedPlayer.username,
                ReportedByPlayerUsername = reporter.username,
                Reason = ReportReasonEnum.Cheating.ToString()
            };

            playerService.ReportPlayer(report);

            Assert.That(TestDatabase.DoesReportExist(reportedPlayer.username), Is.True);
        }

        [Test]
        public void TestReportPlayerUnknownReportedUserDoesNotCreateReport()
        {
            PlayerEntity reporter = TestDataFactory.CreatePlayer();
            Report report = new Report
            {
                ReportedPlayerUsername = TestDataFactory.CreateUniqueUsername(),
                ReportedByPlayerUsername = reporter.username,
                Reason = ReportReasonEnum.Cheating.ToString()
            };

            playerService.ReportPlayer(report);

            Assert.That(TestDatabase.DoesReportExist(report.ReportedPlayerUsername), Is.False);
        }
    }
}
