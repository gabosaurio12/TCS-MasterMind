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
        public void TestSendVerificationCodeCreatesCode()
        {
            codeService.SendVerificationCode(player.player_id);

            Assert.That(TestDatabase.DoesVerificationCodeExist(player.player_id), Is.True);
        }

        [Test]
        public void TestSendVerificationCodeReplacesPreviousCode()
        {
            codeService.SendVerificationCode(player.player_id);
            codeService.SendVerificationCode(player.player_id);

            int result = TestDatabase.CountVerificationCodes(player.player_id);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void TestAuthVerificationCodeWithValidCodeReturnsTrue()
        {
            codeService.SendVerificationCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);

            bool result = codeService.AuthVerificationCode(player.username, code);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestAuthVerificationCodeRemovesUsedCode()
        {
            codeService.SendVerificationCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            codeService.AuthVerificationCode(player.username, code);

            Assert.That(TestDatabase.DoesVerificationCodeExist(player.player_id), Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeWithWrongCodeReturnsFalse()
        {
            codeService.SendVerificationCode(player.player_id);

            bool result = codeService.AuthVerificationCode(player.username, "99999");

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeWithUnknownUserReturnsFalse()
        {
            codeService.SendVerificationCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);

            bool result = codeService.AuthVerificationCode(TestDataFactory.CreateUniqueUsername(), code);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeTwiceReturnsFalse()
        {
            codeService.SendVerificationCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            codeService.AuthVerificationCode(player.username, code);

            bool result = codeService.AuthVerificationCode(player.username, code);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeExpiredThrowsFault()
        {
            codeService.SendVerificationCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ExpiredAge);

            Assert.Throws<FaultException<ErrorMessage>>(
                () => codeService.AuthVerificationCode(player.username, code));
        }

        [Test]
        public void TestAuthVerificationCodeExpiredFaultMessage()
        {
            codeService.SendVerificationCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ExpiredAge);

            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => codeService.AuthVerificationCode(player.username, code));

            Assert.That(exception.Detail.Message, Is.EqualTo(ErrorMessages.VerificationCodeExpired.ToString()));
        }

        [Test]
        public void TestAuthVerificationCodeExpiredRemovesRow()
        {
            codeService.SendVerificationCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ExpiredAge);

            Assert.Throws<FaultException<ErrorMessage>>(
                () => codeService.AuthVerificationCode(player.username, code));

            Assert.That(TestDatabase.DoesVerificationCodeExist(player.player_id), Is.False);
        }

        [Test]
        public void TestAuthVerificationCodeBeforeLimitReturnsTrue()
        {
            codeService.SendVerificationCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ValidAge);

            bool result = codeService.AuthVerificationCode(player.username, code);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestSendVerificationCodeResetsCreatedAt()
        {
            codeService.SendVerificationCode(player.player_id);
            TestDatabase.AgeVerificationCode(player.player_id, ExpiredAge);
            codeService.SendVerificationCode(player.player_id);
            string code = TestDatabase.GetVerificationCode(player.player_id);

            bool result = codeService.AuthVerificationCode(player.username, code);

            Assert.That(result, Is.True);
        }
    }
}
