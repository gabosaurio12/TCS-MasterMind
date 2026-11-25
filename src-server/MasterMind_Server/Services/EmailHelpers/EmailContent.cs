namespace MasterMind_Server.Services.EmailHelpers
{
    internal class EmailContent
    {
        public EmailContent(string subject, string body, bool isHtml)
        {
            Subject = subject;
            Body = body;
            IsHtml = isHtml;
        }

        public string Subject { get; }

        public string Body { get; }

        public bool IsHtml { get; }
    }
}
