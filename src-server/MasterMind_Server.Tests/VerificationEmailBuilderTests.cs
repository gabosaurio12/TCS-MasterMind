using MasterMind_Server.Services.EmailHelpers;
using NUnit.Framework;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    public class VerificationEmailBuilderTests
    {
        private const string Code = "12345";
        private const string Email = "agent@example.com";

        [Test]
        public void TestBuildInsertsVerificationCode()
        {
            var content = VerificationEmailBuilder.Build(Code, Email);

            Assert.That(content.Body, Does.Contain(Code));
        }

        [Test]
        public void TestBuildInsertsUserEmail()
        {
            var content = VerificationEmailBuilder.Build(Code, Email);

            Assert.That(content.Body, Does.Contain(Email));
        }

        [Test]
        public void TestBuildReplacesAllPlaceholders()
        {
            var content = VerificationEmailBuilder.Build(Code, Email);

            Assert.That(content.Body, Does.Not.Contain("{{"));
        }

        [Test]
        public void TestBuildReturnsHtmlContent()
        {
            var content = VerificationEmailBuilder.Build(Code, Email);

            Assert.That(content.IsHtml, Is.True);
        }
    }
}
