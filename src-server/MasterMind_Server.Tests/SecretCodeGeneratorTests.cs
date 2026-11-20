using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services.MatchHelpers;
using NUnit.Framework;
using System;
using System.Linq;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    public class SecretCodeGeneratorTests
    {
        private const int Samples = 500;

        [Test]
        public void TestGenerateReturnsCodeLength()
        {
            var code = SecretCodeGenerator.Generate();
            int codeLength = 4;

            Assert.That(code.Length, Is.EqualTo(codeLength));
        }

        [Test]
        public void TestGenerateOnlyUsesValidColours()
        {
            var allValid = Enumerable.Range(0, Samples)
                .SelectMany(sample => SecretCodeGenerator.Generate())
                .All(colour => Enum.IsDefined(typeof(Colours), colour));

            Assert.That(allValid, Is.True);
        }

        [Test]
        public void TestGenerateEventuallyUsesEveryColour()
        {
            var usedColours = Enumerable.Range(0, Samples)
                .SelectMany(sample => SecretCodeGenerator.Generate())
                .Distinct()
                .Count();

            Assert.That(usedColours, Is.EqualTo(Enum.GetNames(typeof(Colours)).Length));
        }
    }
}
