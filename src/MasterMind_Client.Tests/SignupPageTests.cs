using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace MasterMind_Client.UITests
{
    [TestFixture]
    public class SignupPageTests
    {
        private Application app;
        private UIA3Automation automation;
        private Window window;

        [OneTimeSetUp]
        public void LaunchApp()
        {
            app = Application.Launch(@"D:\mazin\Documents\Codigos\TCS-MasterMind\src\MasterMind_Client\bin\Debug\MasterMind_Client.exe");
            automation = new UIA3Automation();

            var mainWindow = app.GetMainWindow(automation);
            Assert.That(mainWindow, Is.Not.Null, "Can't get the main window of the project.");

            window = mainWindow;

            var signupLink = window.FindFirstDescendant(cf => cf.ByAutomationId("SignupLink"));
            Assert.That(signupLink, Is.Not.Null, "The Hyperlink to SignupPage was not found.");
            signupLink?.AsButton().Invoke();
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

        [Test]
        public void TestUsernameTxtIsWriteableSuccess()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            string expectedUsername = "TestUser";

            Assert.That(usernameBox?.Text, Is.EqualTo(expectedUsername));
        }

        [Test]
        public void TestEmailTxtIsWriteableSuccess()
        {
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt")).AsTextBox();
            emailBox?.Text = "test@email.com";
            string expectedEmail = "test@email.com";

            Assert.That(emailBox?.Text, Is.EqualTo(expectedEmail));
        }

        [Test]
        public void TestRegisterSuccess()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt")).AsTextBox();
            emailBox?.Text = "test@email.com";
            var passwordBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox?.Text = "TestUser123";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window successModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Éxito"));
            }, timeout: TimeSpan.FromSeconds(5)).Result!;

            Assert.That(successModal, Is.Not.Null, "The SuccessNotificationModal didn't appear.");            

            successModal.Close();
        }

        [Test]
        public void TestRegisterWithEmptyFieldsShowsErrorModal()
        {
            window?.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt"))?.AsTextBox().Text = "";
            window?.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt"))?.AsTextBox().Text = "";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(5)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }

        [Test]
        public void TestRegisterWithEmptyUsernameShowsErrorModal()
        {
            window?.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt"))?.AsTextBox().Text = "";
            var emailBox = window?.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt")).AsTextBox();
            emailBox?.Text = "testemail.com";
            var passwordBox = window?.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox?.Text = "TestUser123";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(5)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }

        [Test]
        public void TestRegisterWithEmptyEmailShowsErrorModal()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            window?.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt"))?.AsTextBox().Text = "";
            var passwordBox = window?.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox?.Text = "TestUser123";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(5)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }

        [Test]
        public void TestRegisterWithEmptyPasswordShowsErrorModal()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt"))?.AsTextBox();
            emailBox?.Text = "test@email.com";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(5)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }

        [Test]
        public void TestRegisterEmailWithNoAtShowsErrorModal()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt")).AsTextBox();
            emailBox?.Text = "testemail.com";
            var passwordBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox?.Text = "TestUser123";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(20)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }

        [Test]
        public void TestRegisterEmailWithNoDotComShowsErrorModal()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt")).AsTextBox();
            emailBox?.Text = "test@emailcom";
            var passwordBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox?.Text = "TestUser123";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(20)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }

        [Test]
        public void TestRegisterPasswordLess8CharShowsErrorModal()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt")).AsTextBox();
            emailBox?.Text = "test@email.com";
            var passwordBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox?.Text = "Test";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(20)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }

        [Test]
        public void TestRegisterPasswordWithNoNumbersShowsErrorModal()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt")).AsTextBox();
            emailBox?.Text = "test@email.com";
            var passwordBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox?.Text = "TestUser";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(20)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }

        [Test]
        public void TestRegisterPasswordWithNoUpperCasesShowsErrorModal()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt")).AsTextBox();
            emailBox?.Text = "test@email.com";
            var passwordBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox?.Text = "testuser123";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(20)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }

        [Test]
        public void TestRegisterPasswordWithNoLowerCasesShowsErrorModal()
        {
            var usernameBox = window.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTxt")).AsTextBox();
            usernameBox?.Text = "TestUser";
            var emailBox = window.FindFirstDescendant(cf => cf.ByAutomationId("EmailTxt")).AsTextBox();
            emailBox?.Text = "test@email.com";
            var passwordBox = window.FindFirstDescendant(cf => cf.ByAutomationId("PasswordTxt")).AsTextBox();
            passwordBox?.Text = "TESTUSER123";

            var registerBtn = window?.FindFirstDescendant(cf => cf.ByAutomationId("RegisterBtn")).AsButton();
            registerBtn?.Invoke();

            Window errorModal = Retry.WhileNull(() =>
            {
                var allWindows = app.GetAllTopLevelWindows(automation);
                return allWindows.FirstOrDefault(w => w.Title.Contains("Error"));
            }, timeout: TimeSpan.FromSeconds(20)).Result!;

            Assert.That(errorModal, Is.Not.Null, "The ErrorNotificationModal didn't appear.");

            errorModal.Close();
        }
    }
}