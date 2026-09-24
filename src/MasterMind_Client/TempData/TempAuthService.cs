using MasterMind_Client.TempData.Enum;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MasterMind_Client.Data;

namespace MasterMind_Client.TempData
{
    public static class TempAuthService
    {

        private readonly static MasterMindEntities Context = new MasterMindEntities();
        public static bool AuthPlayer(TempPlayer player)
        {

            var authPlayer = Context.Player.FirstOrDefault(p => p.username == player.Username && p.password == player.Password);

            if (authPlayer == null)
            {
                return false;
            }

            SendVerificationCode(authPlayer.player_id);

            return true;
            
        }

        public static RegistrationResult RegisterPlayer(Player player)
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

        public static (bool, Player) AuthVerificationCode(string username, string code)
        {

            var player = Context.Player.FirstOrDefault(p => p.username == username);
            if (player != null)
            {
                var authCode = Context.VerificationCode.FirstOrDefault(vc => vc.verification_code == code && vc.player_id == player.player_id);
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

        private static bool CreateVerificationCode(int playerId)
        {
            string code = "";
            for (int i = 0; i < 5; i++)
            {
                code += Rand.Next(0,10).ToString();
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


                MessageBox.Show("Código actual: " + code);

                return true;
            }

            return false;
        }
    }
}
