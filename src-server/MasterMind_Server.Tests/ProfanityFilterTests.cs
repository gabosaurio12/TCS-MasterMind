using MasterMind_Server.Services.ChatHelpers;
using NUnit.Framework;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    public class ProfanityFilterTests
    {
        [Test]
        public void TestCensorCleanTextKeepsText()
        {
            Assert.That(ProfanityFilter.Censor("hello class").CensoredText, Is.EqualTo("hello class"));
        }

        [Test]
        public void TestCensorCleanTextHasNoBannedWords()
        {
            Assert.That(ProfanityFilter.Censor("hello class").HasBannedWords, Is.False);
        }

        [Test]
        public void TestCensorBannedWordMasksOneAsteriskPerLetter()
        {
            Assert.That(ProfanityFilter.Censor("you shit").CensoredText, Is.EqualTo("you ****"));
        }

        [Test]
        public void TestCensorBannedWordSetsFlag()
        {
            Assert.That(ProfanityFilter.Censor("mierda").HasBannedWords, Is.True);
        }

        [Test]
        public void TestCensorUpperCaseWordMasksWord()
        {
            Assert.That(ProfanityFilter.Censor("FUCK").CensoredText, Is.EqualTo("****"));
        }

        [Test]
        public void TestCensorAccentedWordMasksWord()
        {
            Assert.That(ProfanityFilter.Censor("CABRÓN").CensoredText, Is.EqualTo("******"));
        }

        [Test]
        public void TestCensorLeetspeakWordMasksWord()
        {
            Assert.That(ProfanityFilter.Censor("m13rd4").CensoredText, Is.EqualTo("******"));
        }

        [Test]
        public void TestCensorPluralWordMasksSuffix()
        {
            Assert.That(ProfanityFilter.Censor("pendejos").CensoredText, Is.EqualTo("********"));
        }

        [Test]
        public void TestCensorWordWithPunctuationKeepsPunctuation()
        {
            Assert.That(ProfanityFilter.Censor("shit, man!").CensoredText, Is.EqualTo("****, man!"));
        }

        [Test]
        public void TestCensorWordInsideAnotherWordKeepsText()
        {
            Assert.That(ProfanityFilter.Censor("Dickens").CensoredText, Is.EqualTo("Dickens"));
        }

        [Test]
        public void TestCensorSeveralWordsMasksAll()
        {
            Assert.That(ProfanityFilter.Censor("puto shit").CensoredText, Is.EqualTo("**** ****"));
        }

        [Test]
        public void TestCensorEmptyTextReturnsEmpty()
        {
            Assert.That(ProfanityFilter.Censor(string.Empty).CensoredText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestCensorNullTextReturnsEmpty()
        {
            Assert.That(ProfanityFilter.Censor(null).CensoredText, Is.EqualTo(string.Empty));
        }
    }
}
