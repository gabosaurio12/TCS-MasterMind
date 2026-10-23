using log4net;
using MasterMind_Server.Data;
using System.Data.Entity.Core;
using System.Linq;
using System.Security.Cryptography;

namespace MasterMind_Server.Repositories
{
    internal class VerificationCodeRepository
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(VerificationCodeRepository));
        private readonly static MasterMindEntities Context = new MasterMindEntities(true);

        private static string GenerateVerificationCode()
        {
            int length = 5;
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            string digits = "0123456789";
            var charCode = new char[length];
            for (int i = 0; i < length; i++)
            {
                charCode[i] = digits[bytes[i] % digits.Length];
            }

            return new string(charCode);
        }

        public static (bool, string) CreateVerificationCode(int playerId)
        {
            string code = GenerateVerificationCode();

            try
            {
                if (Context.VerificationCode.FirstOrDefault(vc => vc.verification_code.Equals(code)) == null)
                {
                    var currentCode = Context.VerificationCode.FirstOrDefault(vc => vc.player_id == playerId);
                    if (currentCode != null)
                    {
                        Context.VerificationCode.Remove(currentCode);
                        Context.SaveChanges();
                    }

                    Context.VerificationCode.Add(
                        new VerificationCode
                        {
                            verification_code = code,
                            player_id = playerId
                        });

                    Context.SaveChanges();

                    return (true, code);
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return (false, "");
        }

        public static bool AuthVerificationCode(string username, string code)
        {
            try
            {
                var player = new PlayerRepository().GetPlayerByUsername(username);
                if (player != null)
                {
                    var authCode = Context.VerificationCode.FirstOrDefault(
                        verificationCode => verificationCode.verification_code == code
                        && verificationCode.player_id == player.player_id);

                    if (authCode != null)
                    {
                        Context.VerificationCode.Remove(authCode);
                        Context.SaveChanges();
                        return true;
                    }
                }
                return false;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw ex;
            }
        }
    }
}
