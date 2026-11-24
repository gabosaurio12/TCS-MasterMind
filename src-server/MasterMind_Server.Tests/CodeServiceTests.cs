using MasterMind_Server.Exceptions;
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
    public class CodeServiceTests
    {
        private static readonly TimeSpan ExpiredAge = TimeSpan.FromMinutes(11);
        private static readonly TimeSpan ValidAge = TimeSpan.FromMinutes(9);

        private CodeService codeService;
        private PlayerEntity player;

        [SetUp]
        public void SetUp()
        {
            TestDatabase.CleanupTestData();
            codeService = new CodeService();
            player = TestDataFactory.CreatePlayer();
        }

        [TearDown]
        public void TearDown()
        {
            TestDatabase.CleanupTestData();
        }

        [Test]
        public void TestSendVerificationCodeSmtpUnreachableThrowsSmtpServerError()
        {
            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => codeService.SendVerificationCode(player.player_id));

            Assert.That(exception.Detail.Message, Is.EqualTo(ErrorMessages.SmtpServerError.ToString()));
        }

        [Test]
        public void TestSendVerificationCodeInvalidEmailThrowsSendEmailError()
        {
            PlayerEntity invalidEmailPlayer = TestDatabase.CreatePlayer(
                TestDataFactory.CreateUniqueUsername(),
                "not-an-email",
                TestDataFactory.DefaultPassword);

            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => codeService.SendVerificationCode(invalidEmailPlayer.player_id));

            Assert.That(exception.Detail.Message, Is.EqualTo(ErrorMessages.SendEmailError.ToString()));
        }

        [Test]
        public void TestSendVerificationCodeCreatesCode()
        {
            SendCode(player.player_id);

            Assert.That(TestDatabase.DoesVerificationCodeExist(player.player_id), Is.True);
        }

        [Test]
        public void TestSendVerificationCodeReplacesPreviousCode()
        {
            SendCode(player.player_id);
            SendCode(player.player_id);

            int result = TestDatabase.CountVerificationCodes(player.player_id);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void TestAuthVerificationCodeWithValidCodeReturnsTrue()
        {
            SendCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);

            bool result = codeService.AuthVerificationCode(player.username, code);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestAuthVerificationCodeRemovesUsedCode()
        {
            SendCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            codeService.AuthVerificationCode(player.username, code);

            Assert.That(TestDatabase.DoesVerificationCodeExist(player.player_id), Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeWithWrongCodeReturnsFalse()
        {
            SendCode(player.player_id);

            bool result = codeService.AuthVerificationCode(player.username, "99999");

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeWithUnknownUserReturnsFalse()
        {
            SendCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);

            bool result = codeService.AuthVerificationCode(TestDataFactory.CreateUniqueUsername(), code);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeTwiceReturnsFalse()
        {
            SendCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            codeService.AuthVerificationCode(player.username, code);

            bool result = codeService.AuthVerificationCode(player.username, code);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeExpiredThrowsFault()
        {
            SendCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ExpiredAge);

            Assert.Throws<FaultException<ErrorMessage>>(
                () => codeService.AuthVerificationCode(player.username, code));
        }

        [Test]
        public void TestAuthVerificationCodeExpiredFaultMessage()
        {
            SendCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ExpiredAge);

            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => codeService.AuthVerificationCode(player.username, code));

            Assert.That(exception.Detail.Message, Is.EqualTo(ErrorMessages.VerificationCodeExpired.ToString()));
        }

        [Test]
        public void TestAuthVerificationCodeExpiredRemovesRow()
        {
            SendCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ExpiredAge);

            Assert.Throws<FaultException<ErrorMessage>>(
                () => codeService.AuthVerificationCode(player.username, code));

            Assert.That(TestDatabase.DoesVerificationCodeExist(player.player_id), Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeBeforeLimitReturnsTrue()
        {
            SendCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ValidAge);

            bool result = codeService.AuthVerificationCode(player.username, code);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestSendVerificationCodeResetsCreatedAt()
        {
            SendCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ExpiredAge);
            SendCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);

            bool result = codeService.AuthVerificationCode(player.username, code);

            Assert.That(result, Is.True);
        }

        private void SendCode(int playerId)
        {
            try
            {
                codeService.SendVerificationCode(playerId);
            }
            catch (FaultException<ErrorMessage>)
            {
                // The test SMTP server is unreachable; the code is stored before the email is attempted.
            }
        }
    }
}
