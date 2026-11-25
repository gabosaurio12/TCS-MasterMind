using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Services.EmailHelpers;
using NUnit.Framework;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    public class InvitationEmailBuilderTests
    {
        private const string Inviter = "testinviter";
        private const string RoomName = "Test Room";
        private const string RoomCode = "654321";
        private const string FriendEmail = "friend@example.com";

        private static RoomInvitationDto CreateInvitation()
        {
            return new RoomInvitationDto
            {
                InviterUsername = Inviter,
                RoomName = RoomName,
                RoomCode = RoomCode,
                FriendEmail = FriendEmail
            };
        }

        [Test]
        public void TestBuildInsertsInviterUsername()
        {
            var content = InvitationEmailBuilder.Build(CreateInvitation());

            Assert.That(content.Body, Does.Contain(Inviter));
        }

        [Test]
        public void TestBuildInsertsRoomName()
        {
            var content = InvitationEmailBuilder.Build(CreateInvitation());

            Assert.That(content.Body, Does.Contain(RoomName));
        }

        [Test]
        public void TestBuildInsertsRoomCode()
        {
            var content = InvitationEmailBuilder.Build(CreateInvitation());

            Assert.That(content.Body, Does.Contain(RoomCode));
        }

        [Test]
        public void TestBuildInsertsUserEmail()
        {
            var content = InvitationEmailBuilder.Build(CreateInvitation());

            Assert.That(content.Body, Does.Contain(FriendEmail));
        }

        [Test]
        public void TestBuildReplacesAllPlaceholders()
        {
            var content = InvitationEmailBuilder.Build(CreateInvitation());

            Assert.That(content.Body, Does.Not.Contain("{{"));
        }

        [Test]
        public void TestBuildReturnsHtmlContent()
        {
            var content = InvitationEmailBuilder.Build(CreateInvitation());

            Assert.That(content.IsHtml, Is.True);
        }
    }
}
