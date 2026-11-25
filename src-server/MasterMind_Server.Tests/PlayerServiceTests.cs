using MasterMind_Server.Contracts;
using MasterMind_Server.Data;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
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
        public void TestRegisterPlayerSmtpUnreachableReturnsSuccessEmailFailed()
        {
            PlayerContract player = TestDataFactory.CreateContractPlayer();

            string result = playerService.RegisterPlayer(player);

            Assert.That(result, Is.EqualTo(RepositoriesMessages.SuccessEmailFailed.ToString()));
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

            Assert.That(result, Is.EqualTo(RepositoriesMessages.UsernameTaken.ToString()));
        }

        [Test]
        public void TestRegisterPlayerDuplicateEmailReturnsEmailTaken()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();
            PlayerContract duplicate = TestDataFactory.CreateContractPlayer();
            duplicate.Email = existingPlayer.email;

            string result = playerService.RegisterPlayer(duplicate);

            Assert.That(result, Is.EqualTo(RepositoriesMessages.EmailTaken.ToString()));
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
        public void TestSetOnlineStatusTrueReturnsSuccess()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();

            string result = playerService.SetOnlineStatus(player.player_id, true);

            Assert.That(result, Is.EqualTo(RepositoriesMessages.Success.ToString()));
        }

        [Test]
        public void TestSetOnlineStatusTrueGetPlayerByIdIsOnline()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            playerService.SetOnlineStatus(player.player_id, true);

            PlayerContract result = playerService.GetPlayerById(player.player_id);

            Assert.That(result.IsOnline, Is.True);
        }

        [Test]
        public void TestSetOnlineStatusFalseGetPlayerByIdIsOffline()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            playerService.SetOnlineStatus(player.player_id, true);
            playerService.SetOnlineStatus(player.player_id, false);

            PlayerContract result = playerService.GetPlayerById(player.player_id);

            Assert.That(result.IsOnline, Is.False);
        }

        [Test]
        public void TestGetPlayerByIdNullOnlineStatusIsOffline()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();

            PlayerContract result = playerService.GetPlayerById(player.player_id);

            Assert.That(result.IsOnline, Is.False);
        }

        [Test]
        public void TestSetOnlineStatusNotFoundPlayerNotFound()
        {
            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => playerService.SetOnlineStatus(TestDataFactory.InvalidPlayerId, true));

            Assert.That(exception.Detail.Message, Is.EqualTo(ErrorMessages.PlayerNotFound.ToString()));
        }

        [Test]
        public void TestResetAllOnlineStatusSetsPlayerOffline()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            playerService.SetOnlineStatus(player.player_id, true);

            new PlayerRepository().ResetAllOnlineStatus();

            Assert.That(playerService.GetPlayerById(player.player_id).IsOnline, Is.False);
        }

        [Test]
        public void TestGetPlayerByIdNotFoundReturnsEmptyPlayer()
        {
            PlayerContract result = playerService.GetPlayerById(TestDataFactory.InvalidPlayerId);

            Assert.That(result.PlayerId, Is.EqualTo(TestDataFactory.InvalidPlayerId));
        }

        [Test]
        public void TestLoginPlayerSmtpUnreachableThrowsSmtpServerError()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();

            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => playerService.LoginPlayer(existingPlayer.username, TestDataFactory.DefaultPassword));

            Assert.That(exception.Detail.Message, Is.EqualTo(ErrorMessages.SmtpServerError.ToString()));
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

            Assert.That(result, Is.EqualTo(RepositoriesMessages.Success.ToString()));
        }

        [Test]
        public void TestUpdatePlayerNotFoundReturnsPlayerNotFound()
        {
            PlayerContract update = TestDataFactory.CreateContractPlayer();
            update.PlayerId = int.MaxValue;

            string result = playerService.UpdatePlayer(update);

            Assert.That(result, Is.EqualTo(RepositoriesMessages.PlayerNotFound.ToString()));
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

            Assert.That(result, Is.EqualTo(RepositoriesMessages.UsernameTaken.ToString()));
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

            Assert.That(result, Is.EqualTo(RepositoriesMessages.EmailTaken.ToString()));
        }

        [Test]
        public void TestChangePasswordSuccess()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();

            string result = playerService.ChangePassword(existingPlayer.username, "NewPassword123!");

            Assert.That(result, Is.EqualTo(RepositoriesMessages.Success.ToString()));
        }

        [Test]
        public void TestChangePasswordUnknownUserReturnsPlayerNotFound()
        {
            string result = playerService.ChangePassword(
                TestDataFactory.CreateUniqueUsername(),
                "NewPassword123!");

            Assert.That(result, Is.EqualTo(RepositoriesMessages.PlayerNotFound.ToString()));
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
                Reason = ReportReason.Cheating.ToString()
            };

            playerService.ReportPlayer(report);

            Assert.That(TestDatabase.DoesReportExist(reportedPlayer.username), Is.True);
        }

        [Test]
        public void TestReportPlayerUnknownReportedUserThrowsPlayerNotFound()
        {
            PlayerEntity reporter = TestDataFactory.CreatePlayer();
            Report report = BuildReport(TestDataFactory.CreateUniqueUsername(), reporter.username, "Cheating");

            AssertReportFault(report, ErrorMessages.PlayerNotFound);
        }

        [Test]
        public void TestReportPlayerUnknownReporterThrowsPlayerNotFound()
        {
            PlayerEntity reportedPlayer = TestDataFactory.CreatePlayer();
            Report report = BuildReport(reportedPlayer.username, TestDataFactory.CreateUniqueUsername(), "Cheating");

            AssertReportFault(report, ErrorMessages.PlayerNotFound);
        }

        [Test]
        public void TestReportPlayerSelfReportThrowsSelfReport()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            Report report = BuildReport(player.username, player.username, "Cheating");

            AssertReportFault(report, ErrorMessages.SelfReport);
        }

        [Test]
        public void TestReportPlayerInvalidReasonThrowsInvalidReportReason()
        {
            PlayerEntity reportedPlayer = TestDataFactory.CreatePlayer();
            PlayerEntity reporter = TestDataFactory.CreatePlayer();
            Report report = BuildReport(reportedPlayer.username, reporter.username, "Spam");

            AssertReportFault(report, ErrorMessages.InvalidReportReason);
        }

        [Test]
        public void TestReportPlayerNullReasonThrowsInvalidReportReason()
        {
            PlayerEntity reportedPlayer = TestDataFactory.CreatePlayer();
            PlayerEntity reporter = TestDataFactory.CreatePlayer();
            Report report = BuildReport(reportedPlayer.username, reporter.username, null);

            AssertReportFault(report, ErrorMessages.InvalidReportReason);
        }

        [Test]
        public void TestReportPlayerNullReportThrowsInvalidReportReason()
        {
            AssertReportFault(null, ErrorMessages.InvalidReportReason);
        }

        [Test]
        public void TestReportPlayerDuplicateThrowsAlreadyReported()
        {
            PlayerEntity reportedPlayer = TestDataFactory.CreatePlayer();
            PlayerEntity reporter = TestDataFactory.CreatePlayer();
            Report report = BuildReport(reportedPlayer.username, reporter.username, "Cheating");
            playerService.ReportPlayer(report);

            AssertReportFault(report, ErrorMessages.AlreadyReported);
        }

        [Test]
        public void TestCreateGuestValidNameReturnsSuccess()
        {
            string result = playerService.CreateGuest(TestDataFactory.CreateUniqueUsername());

            Assert.That(result, Is.EqualTo(RepositoriesMessages.Success.ToString()));
        }

        [Test]
        public void TestCreateGuestMarksPlayerAsGuest()
        {
            string guestName = TestDataFactory.CreateUniqueUsername();
            playerService.CreateGuest(guestName);

            Assert.That(TestDatabase.GetPlayerByUsername(guestName).is_guest, Is.True);
        }

        [Test]
        public void TestCreateGuestDuplicateGuestNameReturnsUsernameTaken()
        {
            string guestName = TestDataFactory.CreateUniqueUsername();
            playerService.CreateGuest(guestName);

            string result = playerService.CreateGuest(guestName);

            Assert.That(result, Is.EqualTo(RepositoriesMessages.UsernameTaken.ToString()));
        }

        [Test]
        public void TestCreateGuestRegisteredNameReturnsUsernameTaken()
        {
            PlayerEntity existingPlayer = TestDataFactory.CreatePlayer();

            string result = playerService.CreateGuest(existingPlayer.username);

            Assert.That(result, Is.EqualTo(RepositoriesMessages.UsernameTaken.ToString()));
        }

        [Test]
        public void TestCreateGuestNameWithSpacesReturnsInvalidGuestName()
        {
            string result = playerService.CreateGuest(TestDataFactory.CreateUniqueUsername() + " guest");

            Assert.That(result, Is.EqualTo(RepositoriesMessages.InvalidGuestName.ToString()));
        }

        [Test]
        public void TestCreateGuestEmptyNameReturnsInvalidGuestName()
        {
            string result = playerService.CreateGuest("   ");

            Assert.That(result, Is.EqualTo(RepositoriesMessages.InvalidGuestName.ToString()));
        }

        [Test]
        public void TestCreateGuestPurgesOfflineGuests()
        {
            PlayerEntity offlineGuest = CreateGuestPlayer();
            TestDatabase.SetOnlineStatus(offlineGuest.player_id, null);

            playerService.CreateGuest(TestDataFactory.CreateUniqueUsername());

            Assert.That(TestDatabase.DoesPlayerExist(offlineGuest.username), Is.False);
        }

        [Test]
        public void TestCreateGuestKeepsOnlineGuests()
        {
            PlayerEntity onlineGuest = CreateGuestPlayer();

            playerService.CreateGuest(TestDataFactory.CreateUniqueUsername());

            Assert.That(TestDatabase.DoesPlayerExist(onlineGuest.username), Is.True);
        }

        [Test]
        public void TestDeleteGuestRemovesPlayer()
        {
            PlayerEntity guest = CreateGuestPlayer();

            playerService.DeleteGuest(guest.player_id);

            Assert.That(TestDatabase.DoesPlayerExist(guest.username), Is.False);
        }

        [Test]
        public void TestDeleteGuestRegisteredPlayerThrowsPlayerNotFound()
        {
            PlayerEntity registeredPlayer = TestDataFactory.CreatePlayer();

            Assert.That(
                () => playerService.DeleteGuest(registeredPlayer.player_id),
                Throws.TypeOf<FaultException<ErrorMessage>>()
                    .With.Property("Detail").Property("Message").EqualTo(ErrorMessages.PlayerNotFound.ToString()));
        }

        [Test]
        public void TestDeleteGuestRemovesReportsAgainstGuest()
        {
            PlayerEntity guest = CreateGuestPlayer();
            TestDatabase.SeedReports(guest.player_id, TestDataFactory.CreatePlayer().player_id, 1);

            playerService.DeleteGuest(guest.player_id);

            Assert.That(TestDatabase.CountReportsAgainst(guest.player_id), Is.EqualTo(0));
        }

        [Test]
        public void TestDeleteGuestAloneInRoomDeletesRoom()
        {
            PlayerEntity guest = CreateGuestPlayer();
            int roomId = CreateRoomFor(guest);

            playerService.DeleteGuest(guest.player_id);

            Assert.That(TestDatabase.DoesRoomExist(roomId), Is.False);
        }

        [Test]
        public void TestDeleteGuestHostWithCompanionPromotesCompanion()
        {
            PlayerEntity guest = CreateGuestPlayer();
            PlayerEntity companion = TestDataFactory.CreatePlayer();
            int roomId = CreateRoomFor(guest);
            new RoomService().JoinRoom(new Room { Name = GetRoomName(roomId) }, companion.username);

            playerService.DeleteGuest(guest.player_id);

            Assert.That(TestDatabase.GetRoomPlayerOneId(roomId), Is.EqualTo(companion.player_id));
        }

        [Test]
        public void TestLoginPlayerGuestUsernameReturnsFalse()
        {
            PlayerEntity guest = CreateGuestPlayer();

            bool result = playerService.LoginPlayer(guest.username, TestDataFactory.DefaultPassword);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestReportPlayerGuestReporterThrowsGuestCannotReport()
        {
            PlayerEntity reportedPlayer = TestDataFactory.CreatePlayer();
            PlayerEntity guest = CreateGuestPlayer();
            Report report = BuildReport(reportedPlayer.username, guest.username, "Cheating");

            AssertReportFault(report, ErrorMessages.GuestCannotReport);
        }

        [Test]
        public void TestReportPlayerGuestReportedCreatesReport()
        {
            PlayerEntity guest = CreateGuestPlayer();
            PlayerEntity reporter = TestDataFactory.CreatePlayer();

            playerService.ReportPlayer(BuildReport(guest.username, reporter.username, "Cheating"));

            Assert.That(TestDatabase.DoesReportExist(guest.username), Is.True);
        }

        private PlayerEntity CreateGuestPlayer()
        {
            string guestName = TestDataFactory.CreateUniqueUsername();
            playerService.CreateGuest(guestName);

            return TestDatabase.GetPlayerByUsername(guestName);
        }

        private static int CreateRoomFor(PlayerEntity owner)
        {
            var room = new Room
            {
                Name = TestDataFactory.CreateUniqueRoomName(),
                Gamemode = TestDataFactory.DefaultGamemode,
                Difficulty = TestDataFactory.DefaultDifficulty,
                PlayerOneUsername = owner.username,
                RoomPrivacy = TestDataFactory.PublicPrivacy
            };

            return new RoomService().CreateRoom(room).Room.RoomId;
        }

        private static string GetRoomName(int roomId)
        {
            return new RoomService().GetRoomById(roomId).Name;
        }

        private static Report BuildReport(string reportedUsername, string reporterUsername, string reason)
        {
            return new Report
            {
                ReportedPlayerUsername = reportedUsername,
                ReportedByPlayerUsername = reporterUsername,
                Reason = reason
            };
        }

        private void AssertReportFault(Report report, ErrorMessages expectedMessage)
        {
            Assert.That(
                () => playerService.ReportPlayer(report),
                Throws.TypeOf<FaultException<ErrorMessage>>()
                    .With.Property("Detail").Property("Message").EqualTo(expectedMessage.ToString()));
        }
    }
}
