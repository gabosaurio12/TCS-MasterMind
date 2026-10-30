using MasterMind_Server.Services;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class CodeServiceTests
    {
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
    }
}
