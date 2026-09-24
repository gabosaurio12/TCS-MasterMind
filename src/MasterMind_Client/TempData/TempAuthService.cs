using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using MasterMind_Client.Data;
using MasterMind_Client.TempData.Enum;
using System.Data.Entity.Core;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace MasterMind_Client.TempData
{
    public static class TempAuthService
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(TempAuthService));

        private readonly static MasterMindEntities Context = new MasterMindEntities(true);
        const string Digits = "0123456789";


        public static bool AuthPlayer(Player player)
        {

            try
            {
                var authPlayer = Context.Player.FirstOrDefault(p => p.username == player.username && p.password == player.password);

                if (authPlayer == null)
                {
                    return false;
                }

                SendVerificationCode(authPlayer.player_id);

                return true;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        public static RegistrationResult RegisterPlayer(Player player)
        {
            try
            {
                if (Context.Player.Any(p => p.username == player.username))
                {
                    return RegistrationResult.UsernameTaken;
                }
                if (Context.Player.Any(p => p.email == player.email))
                {
                    return RegistrationResult.EmailTaken;
                }

                Context.Player.Add(player);
                Context.SaveChanges();

                SendVerificationCode(player.player_id);

                return RegistrationResult.Success;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return RegistrationResult.Error;
        }

        public static (bool, Player) AuthVerificationCode(string username, string code)
        {

            try
            {
                var player = Context.Player.FirstOrDefault(p => p.username == username);
                if (player != null)
                {
                    var authCode = Context.VerificationCode.FirstOrDefault(vc => vc.verification_code == code && vc.player_id == player.player_id);
                    if (authCode != null)
                    {
                        Context.VerificationCode.Remove(authCode);
                        Context.SaveChanges();
                        return (true, player);
                    }
                    return (false, player);
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            return (false, null);
        }

        private static void SendVerificationCode(int playerId)
        {
            var result = CreateVerificationCode(playerId);
            while (!result)
            {
                CreateVerificationCode(playerId);
            }
        }

        private static string GenerateVerificationCode()
        {
            int length = 5;
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var charCode = new char[length];
            for (int i = 0; i < length; i++)
            {
                charCode[i] = Digits[bytes[i] % Digits.Length];
            }

            return new string(charCode);
        }

        private static bool CreateVerificationCode(int playerId)
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

                    return true;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return false;
        }
    }
}
