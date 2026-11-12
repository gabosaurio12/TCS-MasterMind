using log4net;
using MasterMind_Server.Exceptions;
using System;
using System.IO;

namespace MasterMind_Server.Services.ClassHelpers
{
    internal class AvatarStorage
    {
        private const string AvatarDirectoryName = "Avatars";
        private const string AvatarFileNameFormat = "avatar_{0}_{1}_{2}.jpg";
        private const string TimestampFormat = "yyyyMMdd_HHmmssfff";

        private readonly static ILog logger = LogManager.GetLogger(typeof(AvatarStorage));

        public string GetAvatarDirectory()
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AvatarDirectoryName);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory;
        }

        public string BuildFileName(int playerId, string username)
        {
            string safeUsername = SanitizeUsername(username);
            string timestamp = DateTime.Now.ToString(TimestampFormat);

            return string.Format(AvatarFileNameFormat, playerId, safeUsername, timestamp);
        }

        public void Save(byte[] content, string fileName)
        {
            string path = Path.Combine(GetAvatarDirectory(), fileName);

            File.WriteAllBytes(path, content);

            logger.Info("Avatar stored: " + fileName);
        }

        public byte[] Read(string fileName)
        {
            string path = Path.Combine(GetAvatarDirectory(), fileName);

            if (!File.Exists(path))
            {
                logger.Warn("The avatar file was not found: " + fileName);
                return null;
            }

            return File.ReadAllBytes(path);
        }

        public void Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            string path = Path.Combine(GetAvatarDirectory(), fileName);

            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
                logger.Info("Previous avatar deleted: " + fileName);
            }
            catch (IOException ex)
            {
                logger.Warn("The previous avatar could not be deleted: " + fileName, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn("The previous avatar could not be deleted: " + fileName, ex);
            }
        }

        private string SanitizeUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return "unknown";
            }

            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            char[] sanitized = username.ToCharArray();

            for (int index = 0; index < sanitized.Length; index++)
            {
                if (Array.IndexOf(invalidCharacters, sanitized[index]) >= 0)
                {
                    sanitized[index] = '_';
                }
            }

            return new string(sanitized);
        }
    }
}
