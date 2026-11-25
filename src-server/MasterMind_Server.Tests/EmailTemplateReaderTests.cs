using MasterMind_Server.Services.EmailHelpers;
using NUnit.Framework;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    public class EmailTemplateReaderTests
    {
        private const string TemplateFileName = "email_template.html";
        private const string MissingTemplateFileName = "missing_template.html";

        [Test]
        public void TestTryReadExistingTemplateReturnsTrue()
        {
            bool result = EmailTemplateReader.TryRead(TemplateFileName, out string template);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryReadMissingTemplateReturnsFalse()
        {
            bool result = EmailTemplateReader.TryRead(MissingTemplateFileName, out string template);

            Assert.That(result, Is.False);
        }
    }
}
