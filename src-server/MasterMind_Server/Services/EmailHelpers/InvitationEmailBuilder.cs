using MasterMind_Server.Repositories.Dtos;
using System.Net;

namespace MasterMind_Server.Services.EmailHelpers
{
    internal static class InvitationEmailBuilder
    {
        public const string Subject = "MasterMind room invitation";

        private const string TemplateFileName = "email_template.html";
        private const string StampText = "Priority Dispatch // Room Access";
        private const string RotorLabel = "Room Access Code (6 Digits)";
        private const string OutroText = "If you did not expect this invitation, disregard this dispatch and "
            + "maintain radio silence.";

        public static EmailContent Build(RoomInvitationDto invitation)
        {
            string plainBody = BuildPlainBody(invitation);

            if (!EmailTemplateReader.TryRead(TemplateFileName, out string template))
            {
                return new EmailContent(Subject, plainBody, false);
            }

            string intro = $"{WebUtility.HtmlEncode(invitation.InviterUsername)} has dispatched a request for you "
                + $"to join the room \"{WebUtility.HtmlEncode(invitation.RoomName)}\". Enter the following access "
                + "code in the \"Code\" option of the rooms list to join the game:";

            string body = template
                .Replace("{{STAMP}}", StampText)
                .Replace("{{INTRO_TEXT}}", intro)
                .Replace("{{ROTOR_LABEL}}", RotorLabel)
                .Replace("{{ROTOR_VALUE}}", invitation.RoomCode)
                .Replace("{{OUTRO_TEXT}}", OutroText)
                .Replace("{{RECIPIENT_EMAIL}}", WebUtility.HtmlEncode(invitation.FriendEmail));

            return new EmailContent(Subject, body, true);
        }

        private static string BuildPlainBody(RoomInvitationDto invitation)
        {
            return $"{invitation.InviterUsername} invited you to play MasterMind in the room "
                + $"\"{invitation.RoomName}\".\n\n"
                + $"Room code: {invitation.RoomCode}\n\n"
                + "Enter this code in the \"Code\" option of the rooms list to join the game.";
        }
    }
}
