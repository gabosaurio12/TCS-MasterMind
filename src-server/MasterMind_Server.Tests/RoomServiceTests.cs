using MasterMind_Server.Contracts;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services;
using MasterMind_Server.Tests.TestSupport;
using NUnit.Framework;
using System.Collections.Generic;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class RoomServiceTests
    {
        private RoomService roomService;
        private PlayerEntity roomOwner;

        [SetUp]
        public void SetUp()
        {
            TestDatabase.CleanupTestData();
            roomService = new RoomService();
            roomOwner = TestDataFactory.CreatePlayer();
        }

        [TearDown]
        public void TearDown()
        {
            TestDatabase.CleanupTestData();
        }

        [Test]
        public void TestCreateRoomSuccess()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);

            RoomCreationResult result = roomService.CreateRoom(room);

            Assert.That(result.Status, Is.EqualTo(RoomResultEnum.Success.ToString()));
        }

        [Test]
        public void TestCreatePrivateRoomReturnsCode()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PrivatePrivacy);

            RoomCreationResult result = roomService.CreateRoom(room);

            Assert.That(result.Room.RoomCode, Is.Not.Empty);
        }

        [Test]
        public void TestCreateRoomDuplicateNameReturnsRoomNameTaken()
        {
            Room firstRoom = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            roomService.CreateRoom(firstRoom);
            Room duplicateRoom = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            duplicateRoom.Name = firstRoom.Name;

            RoomCreationResult result = roomService.CreateRoom(duplicateRoom);

            Assert.That(result.Status, Is.EqualTo(RoomResultEnum.RoomNameTaken.ToString()));
        }

        [Test]
        public void TestCreateRoomInvalidGamemodeReturnsError()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            room.Gamemode = "UnknownGamemode";

            RoomCreationResult result = roomService.CreateRoom(room);

            Assert.That(result.Status, Is.EqualTo(RoomResultEnum.Error.ToString()));
        }

        [Test]
        public void TestCreateRoomUnknownPlayerReturnsError()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            room.PlayerOneUsername = TestDataFactory.CreateUniqueUsername();

            RoomCreationResult result = roomService.CreateRoom(room);

            Assert.That(result.Status, Is.EqualTo(RoomResultEnum.Error.ToString()));
        }

        [Test]
        public void TestGetRoomByNameSuccess()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            roomService.CreateRoom(room);

            Room result = roomService.GetRoomByName(room.Name);

            Assert.That(result.Name, Is.EqualTo(room.Name));
        }

        [Test]
        public void TestGetRoomByNameNotFoundReturnsEmptyRoom()
        {
            Room result = roomService.GetRoomByName(TestDataFactory.CreateUniqueRoomName());

            Assert.That(result.RoomId, Is.EqualTo(0));
        }

        [Test]
        public void TestGetPublicRoomsIncludesCreatedPublicRoom()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            RoomCreationResult createdRoom = roomService.CreateRoom(room);

            List<Room> result = roomService.GetPublicRooms();

            Assert.That(result.Exists(publicRoom => publicRoom.RoomId == createdRoom.Room.RoomId), Is.True);
        }

        [Test]
        public void TestGetPublicRoomsExcludesPrivateRoom()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PrivatePrivacy);
            RoomCreationResult createdRoom = roomService.CreateRoom(room);

            List<Room> result = roomService.GetPublicRooms();

            Assert.That(result.Exists(publicRoom => publicRoom.RoomId == createdRoom.Room.RoomId), Is.False);
        }

        [Test]
        public void TestJoinRoomSuccess()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            roomService.CreateRoom(room);
            PlayerEntity joiningPlayer = TestDataFactory.CreatePlayer();

            string result = roomService.JoinRoom(room, joiningPlayer.username);

            Assert.That(result, Is.EqualTo(RoomResultEnum.Success.ToString()));
        }

        [Test]
        public void TestJoinRoomUnknownPlayerReturnsError()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            roomService.CreateRoom(room);

            string result = roomService.JoinRoom(room, TestDataFactory.CreateUniqueUsername());

            Assert.That(result, Is.EqualTo(RoomResultEnum.Error.ToString()));
        }

        [Test]
        public void TestJoinRoomFullReturnsRoomIsFull()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            roomService.CreateRoom(room);
            PlayerEntity secondPlayer = TestDataFactory.CreatePlayer();
            PlayerEntity thirdPlayer = TestDataFactory.CreatePlayer();
            roomService.JoinRoom(room, secondPlayer.username);

            string result = roomService.JoinRoom(room, thirdPlayer.username);

            Assert.That(result, Is.EqualTo(RoomResultEnum.RoomIsFull.ToString()));
        }

        [Test]
        public void TestJoinRoomAsMemberReturnsSuccess()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            roomService.CreateRoom(room);

            string result = roomService.JoinRoom(room, roomOwner.username);

            Assert.That(result, Is.EqualTo(RoomResultEnum.Success.ToString()));
        }

        [Test]
        public void TestJoinPrivateRoomReturnsError()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PrivatePrivacy);
            roomService.CreateRoom(room);
            PlayerEntity joiningPlayer = TestDataFactory.CreatePlayer();

            string result = roomService.JoinRoom(room, joiningPlayer.username);

            Assert.That(result, Is.EqualTo(RoomResultEnum.Error.ToString()));
        }

        [Test]
        public void TestJoinRoomWithCodeSuccess()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PrivatePrivacy);
            RoomCreationResult createdRoom = roomService.CreateRoom(room);
            PlayerEntity joiningPlayer = TestDataFactory.CreatePlayer();

            string result = roomService.JoinRoomWithCode(
                room,
                joiningPlayer.username,
                createdRoom.Room.RoomCode);

            Assert.That(result, Is.EqualTo(RoomResultEnum.Success.ToString()));
        }

        [Test]
        public void TestJoinRoomWithWrongCodeReturnsWrongCode()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PrivatePrivacy);
            roomService.CreateRoom(room);
            PlayerEntity joiningPlayer = TestDataFactory.CreatePlayer();

            string result = roomService.JoinRoomWithCode(room, joiningPlayer.username, "000000");

            Assert.That(result, Is.EqualTo(RoomResultEnum.WrongCode.ToString()));
        }

        [Test]
        public void TestJoinPublicRoomWithCodeReturnsError()
        {
            Room room = BuildRoom(roomOwner, TestDataFactory.PublicPrivacy);
            RoomCreationResult createdRoom = roomService.CreateRoom(room);
            PlayerEntity joiningPlayer = TestDataFactory.CreatePlayer();

            string result = roomService.JoinRoomWithCode(
                room,
                joiningPlayer.username,
                createdRoom.Room.RoomCode);

            Assert.That(result, Is.EqualTo(RoomResultEnum.Error.ToString()));
        }

        private Room BuildRoom(PlayerEntity owner, string privacy)
        {
            return new Room
            {
                Name = TestDataFactory.CreateUniqueRoomName(),
                Gamemode = TestDataFactory.DefaultGamemode,
                Difficulty = TestDataFactory.DefaultDifficulty,
                PlayerOneUsername = owner.username,
                RoomPrivacy = privacy
            };
        }
    }
}
