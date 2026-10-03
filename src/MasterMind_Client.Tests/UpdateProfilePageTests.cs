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
    public class UpdateProfilePageTests
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
            NavigateToUpdateProfile();
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

        private void NavigateToUpdateProfile()
        {
            var profileBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("ProfileBtn")).AsButton();
            profileBtn.Invoke();

            var updateBtn = Retry.WhileNull(() =>
                window.FindFirstDescendant(cf => cf.ByAutomationId("UpdateBtn")).AsButton(),
                timeout: TimeSpan.FromSeconds(5)).Result!;
            updateBtn.Invoke();
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

        private void SetUsername(string username)
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PlayerUsernameTxt")).AsTextBox();
            usernameBox.Text = username;
        }

        private void SetEmail(string email)
        {
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PlayerEmailTxt")).AsTextBox();
            emailBox.Text = email;
        }

        private void ClickSave()
        {
            var saveBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("SaveBtn")).AsButton();
            saveBtn.Invoke();
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
        public void TestUpdateProfileEmptyUsernameShowsErrorModal()
        {
            SetUsername("");
            ClickSave();

            Window errorModal = WaitForErrorModal();
            Assert.That(errorModal, Is.Not.Null, "Updating profile with empty username didn't show an error modal.");

            errorModal.Close();
        }

        [Test]
        public void TestUpdateProfileEmptyEmailShowsErrorModal()
        {
            SetEmail("");
            ClickSave();

            Window errorModal = WaitForErrorModal();
            Assert.That(errorModal, Is.Not.Null, "Updating profile with empty email didn't show an error modal.");

            errorModal.Close();
        }

        [Test]
        public void TestUpdateProfileUsernameWithSpacesShowsErrorModal()
        {
            SetUsername("Test User");
            ClickSave();

            Window errorModal = WaitForErrorModal();
            Assert.That(errorModal, Is.Not.Null, "Updating profile with spaces in username didn't show an error modal.");

            errorModal.Close();
        }
    }
}
