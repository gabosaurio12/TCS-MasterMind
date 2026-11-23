using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MasterMind_Client.AvatarManagement
{
    public static class AvatarCache
    {
        private const string ApplicationFolderName = "MasterMind_Client";
        private const string AvatarsFolderName = "Avatars";
        private const string TemporaryFileExtension = ".tmp";

        private static readonly ILog logger = LogManager.GetLogger(typeof(AvatarCache));
        private static readonly object writeLock = new object();
        private static readonly string avatarsRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ApplicationFolderName,
            AvatarsFolderName);
        private static readonly string sessionRoot = Path.Combine(
            avatarsRoot,
            Process.GetCurrentProcess().Id.ToString());

        public static byte[] TryRead(AvatarZone zone, string fileName)
        {
            try
            {
                var path = GetFilePath(zone, fileName);

                return File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                logger.Warn(ex);

                return Array.Empty<byte>();
            }
        }

        public static void Save(AvatarZone zone, string fileName, byte[] content)
        {
            lock (writeLock)
            {
                try
                {
                    var path = GetFilePath(zone, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    if (zone == AvatarZone.Own)
                    {
                        ClearFolder(GetZoneFolder(zone));
                    }

                    var temporaryPath = path + TemporaryFileExtension;
                    File.WriteAllBytes(temporaryPath, content);
                    File.Delete(path);
                    File.Move(temporaryPath, path);
                }
                catch (Exception ex) when (IsFileSystemError(ex))
                {
                    logger.Warn(ex);
                }
            }
        }

        public static void Delete(AvatarZone zone, string fileName)
        {
            try
            {
                File.Delete(GetFilePath(zone, fileName));
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                logger.Warn(ex);
            }
        }

        public static void ClearSession()
        {
            DeleteFolder(sessionRoot);
        }

        public static void DeleteOrphanSessions()
        {
            try
            {
                if (!Directory.Exists(avatarsRoot))
                {
                    return;
                }

                var orphans = Directory.GetDirectories(avatarsRoot).Where(IsOrphanSession).ToList();
                orphans.ForEach(DeleteFolder);
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                logger.Warn(ex);
            }
        }

        private static bool IsOrphanSession(string folder)
        {
            if (!int.TryParse(Path.GetFileName(folder), out var processId))
            {
                return true;
            }

            try
            {
                return Process.GetProcessById(processId).HasExited;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }

        private static string GetZoneFolder(AvatarZone zone)
        {
            return Path.Combine(sessionRoot, zone.ToString());
        }

        private static string GetFilePath(AvatarZone zone, string fileName)
        {
            return Path.Combine(GetZoneFolder(zone), Path.GetFileName(fileName));
        }

        private static void ClearFolder(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.GetFiles(folder).ToList().ForEach(File.Delete);
            }
        }

        private static void DeleteFolder(string folder)
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                logger.Warn(ex);
            }
        }

        private static bool IsFileSystemError(Exception exception)
        {
            return exception is IOException
                || exception is UnauthorizedAccessException
                || exception is ArgumentException
                || exception is NotSupportedException;
        }
    }
}
