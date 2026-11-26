using MasterMind_Server.Data;
using MasterMind_Server.Repositories;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System.Collections.Generic;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class RecordsRepositoryTests
    {
        private RecordsRepository recordsRepository;

        [SetUp]
        public void SetUp()
        {
            TestDatabase.CleanupTestData();
            recordsRepository = new RecordsRepository();
        }

        [TearDown]
        public void TearDown()
        {
            TestDatabase.CleanupTestData();
        }

        [Test]
        public void TestSaveTimeRecordsKeepsBestThreeOrdered()
        {
            Player player = TestDataFactory.CreatePlayer();

            recordsRepository.SaveTimeRecords(BuildBestTries(player.username, 40));
            recordsRepository.SaveTimeRecords(BuildBestTries(player.username, 10));
            recordsRepository.SaveTimeRecords(BuildBestTries(player.username, 30));
            recordsRepository.SaveTimeRecords(BuildBestTries(player.username, 20));

            Assert.That(TestDatabase.GetTimeRecords(player.player_id), Is.EqualTo(new[] { 10, 20, 30 }));
        }

        [Test]
        public void TestSaveTriesRecordsCreatesBestValue()
        {
            Player player = TestDataFactory.CreatePlayer();

            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 5));

            Assert.That(TestDatabase.GetTriesRecords(player.player_id), Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void TestSaveTriesRecordsKeepsBestThreeOrdered()
        {
            Player player = TestDataFactory.CreatePlayer();

            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 8));
            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 5));
            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 7));
            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 6));

            Assert.That(TestDatabase.GetTriesRecords(player.player_id), Is.EqualTo(new[] { 5, 6, 7 }));
        }

        [Test]
        public void TestSaveTriesRecordsIgnoresWorseValueWhenFull()
        {
            Player player = TestDataFactory.CreatePlayer();
            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 3));
            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 4));
            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 5));

            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 9));

            Assert.That(TestDatabase.GetTriesRecords(player.player_id), Is.EqualTo(new[] { 3, 4, 5 }));
        }

        [Test]
        public void TestSaveTriesRecordsIgnoresGuest()
        {
            string username = TestDataFactory.CreateUniqueUsername();
            new PlayerRepository().CreateGuest(username);
            Player guest = TestDatabase.GetPlayerByUsername(username);

            recordsRepository.SaveTriesRecords(BuildBestTries(username, 4));

            Assert.That(TestDatabase.GetTriesRecords(guest.player_id), Is.Empty);
        }

        [Test]
        public void TestGetPlayerRecordsReturnsStoredValue()
        {
            Player player = TestDataFactory.CreatePlayer();
            recordsRepository.SaveTriesRecords(BuildBestTries(player.username, 6));

            var records = new PlayerRepository().GetPlayerRecords(player.username);

            Assert.That(records.PlayerTriesRecords, Is.EqualTo(new[] { 6 }));
        }

        private static IReadOnlyDictionary<string, int> BuildBestTries(string username, int tries)
        {
            return new Dictionary<string, int> { { username, tries } };
        }
    }
}
