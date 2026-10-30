using MasterMind_Server.Data;
using System;

namespace MasterMind_Server.Tests.TestSupport
{
    internal static class TestDataFactory
    {
        public const string Prefix = "servertest_";
        public const string DefaultPassword = "ServertestPassword123!";
        public const string DefaultGamemode = "Tries";
        public const string DefaultDifficulty = "Easy";
        public const string PublicPrivacy = "Public";
        public const string PrivatePrivacy = "Private";
        public const int InvalidPlayerId = 0;
        public static readonly TimeSpan CallbackTimeout = TimeSpan.FromSeconds(5);

        public static string CreateUniqueUsername()
        {
            string suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{Prefix}usr_{suffix}";
        }

        public static string CreateUniqueEmail()
        {
            string suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{Prefix}{suffix}@example.invalid";
        }

        public static string CreateUniqueRoomName()
        {
            string suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{Prefix}rm_{suffix}";
        }

        public static string CreateUniqueCode()
        {
            string suffix = Guid.NewGuid().ToString("N").Substring(0, 5);
            return suffix;
        }

        public static Player CreatePlayer()
        {
            string username = CreateUniqueUsername();
            return TestDatabase.CreatePlayer(username, CreateUniqueEmail(), DefaultPassword);
        }

        public static Contracts.Player CreateContractPlayer()
        {
            return new Contracts.Player
            {
                Username = CreateUniqueUsername(),
                Email = CreateUniqueEmail(),
                Password = DefaultPassword
            };
        }
    }
}
