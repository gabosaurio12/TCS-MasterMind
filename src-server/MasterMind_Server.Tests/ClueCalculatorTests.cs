using MasterMind_Server.Services.MatchHelpers;
using NUnit.Framework;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    public class ClueCalculatorTests
    {
        private static readonly string[] secretCode = { "Blue", "Green", "Red", "Yellow" };

        [Test]
        public void TestCalculateExactMatchAllBlack()
        {
            var clues = ClueCalculator.Calculate(secretCode, secretCode);

            Assert.That(clues, Is.EqualTo(new[] { "Black", "Black", "Black", "Black" }));
        }

        [Test]
        public void TestCalculateNoCoincidenceAllNone()
        {
            var guess = new[] { "White", "Orange", "White", "Orange" };

            var clues = ClueCalculator.Calculate(secretCode, guess);

            Assert.That(clues, Is.EqualTo(new[] { "None", "None", "None", "None" }));
        }

        [Test]
        public void TestCalculateShiftedColoursAllWhite()
        {
            var guess = new[] { "Green", "Blue", "Yellow", "Red" };

            var clues = ClueCalculator.Calculate(secretCode, guess);

            Assert.That(clues, Is.EqualTo(new[] { "White", "White", "White", "White" }));
        }

        [Test]
        public void TestCalculateRepeatedGuessColourMarksOnlyAvailableOnes()
        {
            var guess = new[] { "Red", "Red", "Red", "Red" };

            var clues = ClueCalculator.Calculate(secretCode, guess);

            Assert.That(clues, Is.EqualTo(new[] { "None", "None", "Black", "None" }));
        }

        [Test]
        public void TestCalculateRepeatedSecretColourMarksBlackAndWhite()
        {
            var secretWithRepeats = new[] { "Red", "Red", "Blue", "White" };
            var guess = new[] { "Blue", "Red", "Red", "Orange" };

            var clues = ClueCalculator.Calculate(secretWithRepeats, guess);

            Assert.That(clues, Is.EqualTo(new[] { "White", "Black", "White", "None" }));
        }

        [Test]
        public void TestIsExactMatchWithWhiteClueReturnsFalse()
        {
            var isExact = ClueCalculator.IsExactMatch(new[] { "Black", "Black", "Black", "White" });

            Assert.That(isExact, Is.False);
        }
    }
}
