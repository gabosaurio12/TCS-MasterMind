using System.Net;

namespace MasterMind_Server.Services.EmailHelpers
{
    internal static class VerificationEmailBuilder
    {
        public const string Subject = "Verification Code!";

        private const string TemplateFileName = "email_template.html";
        private const string StampText = "Top Secret // One-Time Use";
        private const string RotorLabel = "Verification Key (5 Digits)";
        private const string IntroText = "Agent, an access request to your deciphering terminal has been intercepted. "
            + "Enter the following sequence into the verification rotors to confirm your identity:";
        private const string OutroText = "<strong>Security protocol:</strong> This code will self-destruct in "
            + "<strong>10 minutes</strong>. If you did not initiate this transmission, disregard this dispatch and "
            + "maintain radio silence.";

        public static EmailContent Build(string code, string email)
        {
            if (!EmailTemplateReader.TryRead(TemplateFileName, out string template))
            {
                return new EmailContent(Subject, code, false);
            }

            string body = template
                .Replace("{{STAMP}}", StampText)
                .Replace("{{INTRO_TEXT}}", IntroText)
                .Replace("{{ROTOR_LABEL}}", RotorLabel)
                .Replace("{{ROTOR_VALUE}}", code)
                .Replace("{{OUTRO_TEXT}}", OutroText)
                .Replace("{{RECIPIENT_EMAIL}}", WebUtility.HtmlEncode(email));

            return new EmailContent(Subject, body, true);
        }
    }
}
