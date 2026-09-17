using MasterMind_Client.TempData.Enum;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMind_Client.TempData
{
    public static class TempAuthService
    {

        public static List<TempPlayer> Players { get; set; } = new List<TempPlayer>();
        private static int playersId = 1;
        public static List<TempVerificationCode> VerificationCodes { get; set; } = new List<TempVerificationCode>();
        private static int codesId = 1;
        private static readonly Random Rand = new Random();

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

            Console.WriteLine(player.Id);
            
            return RegistrationResult.Success;
        }

        private static void SendVerificationCode(int playerId)
        {
            var result = CreateVerificationCode(playersId);
            while (!result)
            {
                CreateVerificationCode(playersId);
            }
        }

        private static bool CreateVerificationCode(int playerId)
        {
            string code = "";
            for (int i = 0; i < 5; i++)
            {
                code += Rand.Next(0,9).ToString();
            }

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

                Console.WriteLine("CODIGO ACTUAL: " + code);

                return true;
            }

            return false;
        }
    }
}
