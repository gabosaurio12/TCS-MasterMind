using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services;
using MasterMind_Server.Services.ClassHelpers;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class AvatarServiceTests
    {
        private const int MaximumAvatarBytes = 102400;
        private const int MaximumInputBytes = 10485760;

        private AvatarService avatarService;

        [SetUp]
        public void SetUp()
        {
            TestDatabase.CleanupTestData();
            AvatarTestFiles.DeleteAll();
            avatarService = new AvatarService();
        }

        [TearDown]
        public void TearDown()
        {
            TestDatabase.CleanupTestData();
            AvatarTestFiles.DeleteAll();
        }

        [Test]
        public void TestSaveAvatarStoresFileUnderMaximumSize()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(800, 800);

            string fileName = avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();

            Assert.That(AvatarTestFiles.GetLength(fileName), Is.LessThanOrEqualTo(MaximumAvatarBytes));
        }

        [Test]
        public void TestSaveAvatarUsesUniqueFileNamePattern()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(120, 120);

            string fileName = avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();

            string pattern = $@"^avatar_{player.player_id}_servertest_usr_[0-9a-f]{{8}}_\d{{8}}_\d{{9}}\.jpg$";
            Assert.That(Regex.IsMatch(fileName, pattern), Is.True, fileName);
        }

        [Test]
        public void TestSaveAvatarStoresFileNameInDatabase()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(120, 120);

            string fileName = avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();

            Assert.That(TestDatabase.GetAvatarPath(player.player_id), Is.EqualTo(fileName));
        }

        [Test]
        public void TestSaveAvatarDeletesPreviousAvatar()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(120, 120);

            string firstFileName = avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();
            string secondFileName = avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();

            Assert.That(AvatarTestFiles.Exists(firstFileName), Is.False);
        }

        [Test]
        public void TestSaveAvatarKeepsTheNewAvatarAfterReplacingThePreviousOne()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(120, 120);

            avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();
            string secondFileName = avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();

            Assert.That(AvatarTestFiles.Exists(secondFileName), Is.True);
        }

        [Test]
        public void TestSaveAvatarRollsBackTheFileWhenTheDatabaseFails()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(120, 120);

            var stubRepository = new StubPlayerRepository
            {
                UpdateAvatarPathResult = RepositoriesMessages.Error.ToString()
            };
            var service = new AvatarService(stubRepository, new AvatarImageProcessor(), new AvatarStorage());

            Assert.Throws<FaultException<ErrorMessage>>(
                () => service.SaveAvatar(player.player_id, image).GetAwaiter().GetResult());
        }

        [Test]
        public void TestSaveAvatarRemovesTheStoredFileWhenTheDatabaseFails()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(120, 120);

            var stubRepository = new StubPlayerRepository
            {
                UpdateAvatarPathResult = RepositoriesMessages.Error.ToString()
            };
            var service = new AvatarService(stubRepository, new AvatarImageProcessor(), new AvatarStorage());

            try
            {
                service.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();
            }
            catch (FaultException<ErrorMessage>)
            {
            }

            Assert.That(DirectoryContainsAvatarFiles(), Is.False);
        }

        [Test]
        public void TestSaveAvatarRejectsImageLargerThanTenMegabytes()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreateOversizedContent(MaximumInputBytes + 1);

            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult());

            Assert.That(exception.Detail.Message, Is.EqualTo("ImageTooLarge"));
        }

        [Test]
        public void TestSaveAvatarRejectsInvalidImageBytes()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreateInvalidImage();

            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult());

            Assert.That(exception.Detail.Message, Is.EqualTo("InvalidImage"));
        }

        [Test]
        public void TestSaveAvatarReturnsPlayerNotFoundForUnknownPlayer()
        {
            byte[] image = AvatarImageFactory.CreatePng(120, 120);

            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => avatarService.SaveAvatar(TestDataFactory.InvalidPlayerId, image).GetAwaiter().GetResult());

            Assert.That(exception.Detail.Message, Is.EqualTo("PlayerNotFound"));
        }

        [Test]
        public void TestGetAvatarReturnsStoredAvatar()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(120, 120);

            string fileName = avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();
            AvatarImage result = avatarService.GetAvatar(player.player_id).GetAwaiter().GetResult();

            Assert.That(result.FileName, Is.EqualTo(fileName));
        }

        [Test]
        public void TestGetAvatarReturnsTheStoredContent()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(120, 120);

            avatarService.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();
            AvatarImage result = avatarService.GetAvatar(player.player_id).GetAwaiter().GetResult();

            Assert.That(result.Content.Length, Is.GreaterThan(0));
        }

        [Test]
        public void TestGetAvatarWithoutStoredAvatarReturnsEmptyContent()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();

            AvatarImage result = avatarService.GetAvatar(player.player_id).GetAwaiter().GetResult();

            Assert.That(result.Content.Length, Is.EqualTo(0));
        }

        [Test]
        public void TestGetAvatarForUnknownPlayerReturnsPlayerNotFound()
        {
            var exception = Assert.Throws<FaultException<ErrorMessage>>(
                () => avatarService.GetAvatar(TestDataFactory.InvalidPlayerId).GetAwaiter().GetResult());

            Assert.That(exception.Detail.Message, Is.EqualTo("PlayerNotFound"));
        }

        [Test]
        public void TestUpdateAvatarPathStoresFileNameInDatabase()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            var repository = new PlayerRepository();

            string result = repository.UpdateAvatarPath(player.player_id, "avatar_test.jpg");

            Assert.That(result, Is.EqualTo(RepositoriesMessages.Success.ToString()));
        }

        [Test]
        public void TestUpdateAvatarPathForUnknownPlayerReturnsPlayerNotFound()
        {
            var repository = new PlayerRepository();

            string result = repository.UpdateAvatarPath(TestDataFactory.InvalidPlayerId, "avatar_test.jpg");

            Assert.That(result, Is.EqualTo(RepositoriesMessages.PlayerNotFound.ToString()));
        }

        [Test]
        public void TestAvatarServiceOverNetTcpSavesAndReturnsAvatar()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(200, 200);

            using (var host = new AvatarServiceHost())
            {
                host.Start();

                using (AvatarServiceClient client = host.CreateClient())
                {
                    string fileName = client.Service.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();
                    AvatarImage result = client.Service.GetAvatar(player.player_id).GetAwaiter().GetResult();

                    Assert.That(result.FileName, Is.EqualTo(fileName));
                }
            }
        }

        [Test]
        public void TestAvatarServiceOverNetTcpAcceptsLargeImageContent()
        {
            PlayerEntity player = TestDataFactory.CreatePlayer();
            byte[] image = AvatarImageFactory.CreatePng(900, 900);

            using (var host = new AvatarServiceHost())
            {
                host.Start();

                using (AvatarServiceClient client = host.CreateClient())
                {
                    string fileName = client.Service.SaveAvatar(player.player_id, image).GetAwaiter().GetResult();

                    Assert.That(fileName, Is.Not.Empty);
                }
            }
        }

        private static bool DirectoryContainsAvatarFiles()
        {
            string directory = AvatarTestFiles.GetDirectory();

            if (!Directory.Exists(directory))
            {
                return false;
            }

            return Directory.GetFiles(directory).Length > 0;
        }
    }
}
