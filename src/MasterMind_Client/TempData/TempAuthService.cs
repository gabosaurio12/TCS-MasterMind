using BCrypt.Net;
using log4net;
using MasterMind_Client.Data;
using MasterMind_Client.TempData.Enum;
using System.Data.Entity.Core;
using System.Linq;
using System.Security.Cryptography;

namespace MasterMind_Client.TempData
{
    public static class TempAuthService
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(TempAuthService));

        private readonly static MasterMindEntities Context = new MasterMindEntities(true);


        public static bool AuthPlayer(Player player)
        {

            try
            {
                var authPlayer = Context.Player.FirstOrDefault(p => p.username == player.username);

                if (authPlayer == null)
                {
                    return false;
                }
                try
                {
                    if (BCrypt.Net.BCrypt.Verify(player.password, authPlayer.password))
                    {
                        SendVerificationCode(authPlayer.player_id);
                        return true;
                    }
                }
                catch (SaltParseException ex)
                {
                    logger.Error(ex);
                }
                
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        public static PlayerRegistrationResultEnum RegisterPlayer(Player player)
        {
            try
            {
                if (Context.Player.Any(p => p.username == player.username))
                {
                    return PlayerRegistrationResultEnum.UsernameTaken;
                }
                if (Context.Player.Any(p => p.email == player.email))
                {
                    return PlayerRegistrationResultEnum.EmailTaken;
                }

                player.password = BCrypt.Net.BCrypt.HashPassword(player.password);

                Context.Player.Add(player);
                Context.SaveChanges();

                SendVerificationCode(player.player_id);

                return PlayerRegistrationResultEnum.Success;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return PlayerRegistrationResultEnum.Error;
        }

        public static (bool, Player) AuthVerificationCode(string username, string code)
        {

            try
            {
                var player = TempPlayerService.GetPlayerByUsername(username);
                if (player != null)
                {
                    var authCode = Context.VerificationCode.FirstOrDefault(
                        verificationCode => verificationCode.verification_code == code 
                        && verificationCode.player_id == player.player_id);

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

            string digits = "0123456789";
            var charCode = new char[length];
            for (int i = 0; i < length; i++)
            {
                charCode[i] = digits[bytes[i] % digits.Length];
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
