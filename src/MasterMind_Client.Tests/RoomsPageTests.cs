using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MasterMind_Client.UITests
{
    [TestFixture]
    public class RoomsPageTests
    {
        private const string ExePath = @"D:\mazin\Documents\Codigos\TCS-MasterMind\src\MasterMind_Client\bin\Debug\MasterMind_Client.exe";
        private const string TestUsername = "TestUser";
        private const string TestPassword = "TestUser123";
        private const int VerificationCodeLength = 5;

        private Application app;
        private UIA3Automation automation;
        private Window window;

        [OneTimeSetUp]
        public void LaunchApp()
        {
            app = Application.Launch(ExePath);
            automation = new UIA3Automation();

            var mainWindow = app.GetMainWindow(automation);
            Assert.That(mainWindow, Is.Not.Null, "Can't get the main window of the project.");

            window = mainWindow;

            Login();
            NavigateToRooms();
        }

        [TearDown]
        public void CloseAnyLeftoverModal()
        {
            var leftoverModal = window.ModalWindows.FirstOrDefault();
            while (leftoverModal != null)
            {
                leftoverModal?.Close();
                leftoverModal = window.ModalWindows.FirstOrDefault();
            }
        }

        [OneTimeTearDown]
        public void CloseApp()
        {
            app.Close();
            automation.Dispose();
        }

        private void Login()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox.Text = TestUsername;
            var passwordBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox.Text = TestPassword;

            var loginBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("LoginBtn")).AsButton();
            loginBtn.Invoke();

            var verificationModal = FindVerificationModal();
            Assert.That(verificationModal, Is.Not.Null, "The VerificationCodeModal didn't appear.");

            string code = GetVerificationCodeFromDb(TestUsername);
            Assert.That(code, Is.Not.Null, $"No verification code found in DB for user {TestUsername}.");

            for (int i = 0; i < VerificationCodeLength; i++)
            {
                var codeBox = verificationModal.FindFirstDescendant(cf => cf.ByAutomationId($"CodeI{i}Txt")).AsTextBox();
                codeBox.Text = code[i].ToString();
            }

            var continueBtn = verificationModal.FindFirstDescendant(cf => cf.ByAutomationId("ContinueBtn")).AsButton();
            continueBtn.Invoke();

            var playBtn = Retry.WhileNull(() =>
                window.FindFirstDescendant(cf => cf.ByAutomationId("PlayBtn")),
                timeout: TimeSpan.FromSeconds(5)).Result!;
            Assert.That(playBtn, Is.Not.Null, "Login didn't reach MainPage.");
        }

        private Window FindVerificationModal()
        {
            return Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w =>
                    w.FindFirstDescendant(cf => cf.ByAutomationId("CodeI0Txt")) != null);
            }, timeout: TimeSpan.FromSeconds(5)).Result!;
        }

        private void NavigateToRooms()
        {
            var playBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("PlayBtn")).AsButton();
            playBtn.Invoke();
        }

        private void ResetToRoomsPage()
        {
            var backBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("BackBtn")).AsButton();
            backBtn.Invoke();

            var playBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("PlayBtn")).AsButton();
            playBtn.Invoke();
        }

        private string GetVerificationCodeFromDb(string username)
        {
            var connectionString = GetDbConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT TOP 1 vc.verification_code FROM VerificationCode vc " +
                    "INNER JOIN Player p ON p.player_id = vc.player_id " +
                    "WHERE p.username = @username " +
                    "ORDER BY vc.verification_code_id DESC", connection);
                command.Parameters.AddWithValue("@username", username);
                return command.ExecuteScalar() as string;
            }
        }

        private string GetDbConnectionString()
        {
            var envPath = Path.Combine(Path.GetDirectoryName(ExePath), ".env");
            var env = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(envPath))
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    env[parts[0].Trim()] = parts[1].Trim();
                }
            }

            return $@"Data Source=(localdb)\MSSQLLocalDB;InitialCatalog={env["DB_NAME"]};User ID={env["DB_USER"]};Password={env["DB_PASSWORD"]};TrustServerCertificate=True;Encrypt=False;";
        }

        private int GetCurrentPlayerId()
        {
            var connectionString = GetDbConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT player_id FROM Player WHERE username = @username", connection);
                command.Parameters.AddWithValue("@username", TestUsername);
                return (int)command.ExecuteScalar();
            }
        }

        private string GetAvailableRoomName()
        {
            int playerId = GetCurrentPlayerId();
            var connectionString = GetDbConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT TOP 1 match_room_name FROM MatchRoom " +
                    "WHERE player_two_id IS NULL AND player_one_id != @playerId " +
                    "ORDER BY match_room_id", connection);
                command.Parameters.AddWithValue("@playerId", playerId);
                return command.ExecuteScalar() as string;
            }
        }

        private string GetFullRoomName()
        {
            int playerId = GetCurrentPlayerId();
            var connectionString = GetDbConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT TOP 1 match_room_name FROM MatchRoom " +
                    "WHERE player_two_id IS NOT NULL AND player_two_id != @playerId AND player_one_id != @playerId " +
                    "ORDER BY match_room_id", connection);
                command.Parameters.AddWithValue("@playerId", playerId);
                return command.ExecuteScalar() as string;
            }
        }

        private string GetOwnRoomName()
        {
            int playerId = GetCurrentPlayerId();
            var connectionString = GetDbConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT TOP 1 match_room_name FROM MatchRoom WHERE player_one_id = @playerId ORDER BY match_room_id", connection);
                command.Parameters.AddWithValue("@playerId", playerId);
                return command.ExecuteScalar() as string;
            }
        }

        private void ReleaseRoom(string roomName)
        {
            var connectionString = GetDbConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "UPDATE MatchRoom SET player_two_id = NULL WHERE match_room_name = @roomName", connection);
                command.Parameters.AddWithValue("@roomName", roomName);
                command.ExecuteNonQuery();
            }
        }

        private void SelectRoom(string roomName)
        {
            var roomLabel = window.FindFirstDescendant(cf => cf.ByAutomationId("RoomNameTxt").And(cf.ByText(roomName)));
            Assert.That(roomLabel, Is.Not.Null, $"Room '{roomName}' was not found in RoomsPage.");
            roomLabel.Click();
        }

        private void ClickJoin()
        {
            var joinBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("JoinBtn")).AsButton();
            joinBtn.Invoke();
        }

        private void Escape()
        {
            var escapeBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("EscapeBtn")).AsButton();
            escapeBtn.Invoke();
        }

        private AutomationElement WaitForRoomPage()
        {
            return Retry.WhileNull(() =>
                window.FindFirstDescendant(cf => cf.ByAutomationId("RoomTitleLbl")),
                timeout: TimeSpan.FromSeconds(5)).Result!;
        }

        private Window WaitForErrorModal()
        {
            return Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(5)).Result!;
        }

        [Test]
        public void TestJoinRoomSuccess()
        {
            string roomName = GetAvailableRoomName();
            Assert.That(roomName, Is.Not.Null, "No room with one free slot found in DB.");

            try
            {
                SelectRoom(roomName);
                ClickJoin();

                var title = WaitForRoomPage();
                Assert.That(title.Name, Is.EqualTo(roomName), "RoomPage title doesn't match the joined room.");
            }
            finally
            {
                ReleaseRoom(roomName);
            }
        }

        [Test]
        public void TestJoinRoomShowsUsernameInSlot()
        {
            string roomName = GetAvailableRoomName();
            Assert.That(roomName, Is.Not.Null, "No room with one free slot found in DB.");

            try
            {
                SelectRoom(roomName);
                ClickJoin();

                WaitForRoomPage();

                var usernames = window.FindAllDescendants(cf => cf.ByAutomationId("UsernameTxt"))
                    .Select(u => u.AsTextBox().Text)
                    .ToList();

                Assert.That(usernames, Does.Contain(TestUsername), "Current player username is not shown in any player slot.");
            }
            finally
            {
                ReleaseRoom(roomName);
            }
        }

        [Test]
        public void TestJoinSameRoomTwiceReenters()
        {
            string roomName = GetAvailableRoomName();
            Assert.That(roomName, Is.Not.Null, "No room with one free slot found in DB.");

            try
            {
                SelectRoom(roomName);
                ClickJoin();
                WaitForRoomPage();

                Escape();

                SelectRoom(roomName);
                ClickJoin();

                var title = WaitForRoomPage();
                Assert.That(title.Name, Is.EqualTo(roomName), "Re-joining the same room didn't navigate to RoomPage.");
            }
            finally
            {
                ReleaseRoom(roomName);
            }
        }

        [Test]
        public void TestEscapeReturnsToRoomsPage()
        {
            string roomName = GetAvailableRoomName();
            Assert.That(roomName, Is.Not.Null, "No room with one free slot found in DB.");

            try
            {
                SelectRoom(roomName);
                ClickJoin();
                WaitForRoomPage();

                Escape();

                var joinBtn = Retry.WhileNull(() =>
                    window.FindFirstDescendant(cf => cf.ByAutomationId("JoinBtn")),
                    timeout: TimeSpan.FromSeconds(5)).Result!;

                Assert.That(joinBtn, Is.Not.Null, "Escape didn't return to RoomsPage.");
            }
            finally
            {
                ReleaseRoom(roomName);
            }
        }

        [Test]
        public void TestJoinRoomWithoutSelectionDoesNotNavigate()
        {
            ResetToRoomsPage();

            ClickJoin();

            var joinBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("JoinBtn"));
            Assert.That(joinBtn, Is.Not.Null, "Join without selection navigated away from RoomsPage.");
        }

        [Test]
        public void TestJoinOwnRoomShowsErrorModal()
        {
            string roomName = GetOwnRoomName();
            Assert.That(roomName, Is.Not.Null, "No own room found in DB.");

            SelectRoom(roomName);
            ClickJoin();

            Window errorModal = WaitForErrorModal();
            Assert.That(errorModal, Is.Not.Null, "Joining own room didn't show an error modal.");

            errorModal.Close();
        }

        [Test]
        public void TestJoinFullRoomShowsErrorModal()
        {
            string roomName = GetFullRoomName();
            Assert.That(roomName, Is.Not.Null, "No full room found in DB.");

            SelectRoom(roomName);
            ClickJoin();

            Window errorModal = WaitForErrorModal();
            Assert.That(errorModal, Is.Not.Null, "Joining a full room didn't show an error modal.");

            errorModal.Close();
        }
    }
}
