using System;
using System.IO;

namespace MasterMind_Server.Tests.TestSupport
{
    internal static class AvatarTestFiles
    {
        private const string AvatarDirectoryName = "Avatars";

        public static string GetDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AvatarDirectoryName);
        }

        public static string GetPath(string fileName)
        {
            return Path.Combine(GetDirectory(), fileName);
        }

        public static bool Exists(string fileName)
        {
            return File.Exists(GetPath(fileName));
        }

        public static long GetLength(string fileName)
        {
            return new FileInfo(GetPath(fileName)).Length;
        }

        public static void DeleteAll()
        {
            string directory = GetDirectory();

            if (!Directory.Exists(directory))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(directory))
            {
                File.Delete(file);
            }
        }
    }
}
