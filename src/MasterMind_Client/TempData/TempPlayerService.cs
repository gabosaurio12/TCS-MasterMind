using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMind_Client.TempData
{
    public static class TempPlayerService
    {
        private readonly static List<TempPlayer> reportedPlayers = new List<TempPlayer>();

        public static void UpdatePlayer(TempPlayer updatedPlayer)
        {
            var player = TempAuthService.Players.FirstOrDefault(p => p.Id == updatedPlayer.Id);

            if (player != null)
            {
                player.Username = updatedPlayer.Username;
                player.Email = updatedPlayer.Email;
            }

            CurrentPlayer.Instance.SetCurrentPlayer(TempAuthService.Players.FirstOrDefault(p => p.Id == updatedPlayer.Id));
        }

        public static void ReportPlayer(string username)
        {
            var player = TempAuthService.Players.FirstOrDefault(p => p.Username == username);
            if (player != null)
            {
                reportedPlayers.Add(player);
            }
        }
    }
}
