using log4net;
using System;
using System.IO;

namespace MasterMind_Server.Services.EmailHelpers
{
    internal static class EmailTemplateReader
    {
        private const string TemplateDirectoryName = "Emails";

        private static readonly ILog logger = LogManager.GetLogger(typeof(EmailTemplateReader));

        public static bool TryRead(string fileName, out string template)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemplateDirectoryName, fileName);

            try
            {
                template = File.ReadAllText(path);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                logger.Warn("Could not read the email template " + fileName + ".", ex);
                template = string.Empty;
                return false;
            }
        }
    }
}
