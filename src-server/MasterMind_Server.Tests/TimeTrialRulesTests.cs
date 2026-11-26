using MasterMind_Server.Services.MatchHelpers;
using NUnit.Framework;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    public class TimeTrialRulesTests
    {
        [Test]
        public void TestGetRoundSecondsEasyReturnsSixty()
        {
            Assert.That(TimeTrialRules.GetRoundSeconds("Easy"), Is.EqualTo(60));
        }

        [Test]
        public void TestGetRoundSecondsNormalReturnsThirty()
        {
            Assert.That(TimeTrialRules.GetRoundSeconds("Normal"), Is.EqualTo(30));
        }

        [Test]
        public void TestGetRoundSecondsHardReturnsFifteen()
        {
            Assert.That(TimeTrialRules.GetRoundSeconds("Hard"), Is.EqualTo(15));
        }

        [Test]
        public void TestGetRoundSecondsEnigmaReturnsTen()
        {
            Assert.That(TimeTrialRules.GetRoundSeconds("Enigma"), Is.EqualTo(10));
        }

        [Test]
        public void TestGetRoundSecondsUnknownReturnsNormal()
        {
            Assert.That(TimeTrialRules.GetRoundSeconds(null), Is.EqualTo(30));
        }

        [Test]
        public void TestIsTimeTrialTriesReturnsFalse()
        {
            Assert.That(TimeTrialRules.IsTimeTrial("Tries"), Is.False);
        }
    }
}
