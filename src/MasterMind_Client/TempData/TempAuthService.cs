using MasterMind_Client.TempData.Enum;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Security.Cryptography;

namespace MasterMind_Client.TempData
{
    public static class TempAuthService
    {

        public static List<TempPlayer> Players { get; set; } = new List<TempPlayer>();
        private static int playersId = 0;
        public static List<TempVerificationCode> VerificationCodes { get; set; } = new List<TempVerificationCode>();
        private static int codesId = 0;
        const string Digits = "0123456789";


        public static bool AuthPlayer(TempPlayer player)
        {
            var authPlayer = Players.FirstOrDefault(p => p.Username == player.Username && p.Password == player.Password);
            if (authPlayer == null)
            {
                return false;
            }

            SendVerificationCode(authPlayer.Id);

            return true;
        }

        public static RegistrationResult RegisterPlayer(TempPlayer player)
        {
            if (Players.Any(p => p.Username == player.Username))
            {
                return RegistrationResult.UsernameTaken;
            }
            if (Players.Any(p => p.Email == player.Email))
            {
                return RegistrationResult.EmailTaken;
            }

            player.Id = ++playersId;
            Players.Add(player);

            SendVerificationCode(player.Id);
            
            return RegistrationResult.Success;
        }

        public static RegistrationResult MockRegisterPlayer(TempPlayer player)
        {
            if (Players.Any(p => p.Username == player.Username))
            {
                return RegistrationResult.UsernameTaken;
            }
            if (Players.Any(p => p.Email == player.Email))
            {
                return RegistrationResult.EmailTaken;
            }

            player.Id = ++playersId;
            Players.Add(player);

            return RegistrationResult.Success;
        }

        public static (bool, TempPlayer) AuthVerificationCode(string username, string code)
        {
            var player = Players.FirstOrDefault(p => p.Username == username);
            if (player != null)
            {
                var authCode = VerificationCodes.FirstOrDefault(vc => vc.Code == code && vc.PlayerId == player.Id);
                if (authCode != null)
                {
                    return (true, player);
                }
            }
            return (false, player);
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

            if (VerificationCodes.FirstOrDefault(vc => vc.Code.Equals(code)) == null)
            {
                var currentCode = VerificationCodes.FirstOrDefault(vc => vc.PlayerId == playerId);
                VerificationCodes.Remove(currentCode);
                VerificationCodes.Add(
                    new TempVerificationCode
                    {
                        Id = ++codesId,
                        Code = code,
                        PlayerId = playerId
                    });


                MessageBox.Show("Código actual: " + code);

                return true;
            }

            return false;
        }
    }
}
