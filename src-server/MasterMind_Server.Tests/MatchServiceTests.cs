using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Services;
using NUnit.Framework;
using System.ServiceModel;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    public class MatchServiceTests
    {
        private MatchService matchService;

        [SetUp]
        public void SetUp()
        {
            matchService = new MatchService();
        }

        [Test]
        public void TestValidateTryShortCodeThrowsInvalidCode()
        {
            var codeTry = new CodeTry { Code = new[] { "Blue", "Red" } };

            var fault = Assert.Throws<FaultException<ErrorMessage>>(
                () => matchService.ValidateTry("servertest_user", codeTry));

            Assert.That(fault.Detail.Message, Is.EqualTo(ErrorMessages.InvalidCode.ToString()));
        }

        [Test]
        public void TestValidateTryUnknownColourThrowsInvalidCode()
        {
            var codeTry = new CodeTry { Code = new[] { "Blue", "Red", "Purple", "Green" } };

            var fault = Assert.Throws<FaultException<ErrorMessage>>(
                () => matchService.ValidateTry("servertest_user", codeTry));

            Assert.That(fault.Detail.Message, Is.EqualTo(ErrorMessages.InvalidCode.ToString()));
        }

        [Test]
        public void TestValidateTryNullCodeThrowsInvalidCode()
        {
            var fault = Assert.Throws<FaultException<ErrorMessage>>(
                () => matchService.ValidateTry("servertest_user", null));

            Assert.That(fault.Detail.Message, Is.EqualTo(ErrorMessages.InvalidCode.ToString()));
        }
    }
}
