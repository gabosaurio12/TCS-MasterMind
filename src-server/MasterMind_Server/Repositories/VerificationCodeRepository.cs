using log4net;
using MasterMind_Server.Data;
using MasterMind_Server.Exceptions;
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;

namespace MasterMind_Server.Repositories
{
    internal static class VerificationCodeRepository
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(VerificationCodeRepository));
        private const int CodeLength = 5;
        private const int CodeLifetimeMinutes = 10;
        private const string Digits = "0123456789";

        private static string GenerateVerificationCode()
        {
            var bytes = new byte[CodeLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var charCode = new char[CodeLength];
            for (int i = 0; i < CodeLength; i++)
            {
                charCode[i] = Digits[bytes[i] % Digits.Length];
            }

            return new string(charCode);
        }

        private static DateTime GetExpirationThreshold()
        {
            return DateTime.UtcNow.AddMinutes(-CodeLifetimeMinutes);
        }

        public static string CreateVerificationCode(int playerId)
        {
            string code = GenerateVerificationCode();
            var threshold = GetExpirationThreshold();

            using (var context = new MasterMindEntities(true))
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var staleCodes = context.VerificationCode
                    .Where(vc => vc.player_id == playerId || vc.created_at < threshold);
                context.VerificationCode.RemoveRange(staleCodes);

                context.VerificationCode.Add(
                    new VerificationCode
                    {
                        verification_code = code,
                        player_id = playerId,
                        created_at = DateTime.UtcNow
                    });

                context.SaveChanges();
                transaction.Commit();
            }

            return code;
        }

        public static bool AuthVerificationCode(string username, string code)
        {
            var player = new PlayerRepository().GetPlayerByUsername(username);
            if (player == null)
            {
                return false;
            }

            bool expired;
            bool matched = TryConsumeCode(player.player_id, code, out expired);

            if (expired)
            {
                logger.Warn("A verification code expired before being used.");
                throw new InvalidOperationException(ErrorMessages.VerificationCodeExpired.ToString());
            }

            return matched;
        }

        private static bool TryConsumeCode(int playerId, string code, out bool expired)
        {
            var threshold = GetExpirationThreshold();
            expired = false;

            using (var context = new MasterMindEntities(true))
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var authCode = context.VerificationCode.FirstOrDefault(
                    vc => vc.verification_code == code && vc.player_id == playerId);

                if (authCode == null)
                {
                    return false;
                }

                expired = authCode.created_at < threshold;
                context.VerificationCode.Remove(authCode);
                context.SaveChanges();
                transaction.Commit();

                return !expired;
            }
        }
    }
}
