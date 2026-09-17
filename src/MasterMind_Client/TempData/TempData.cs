using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMind_Client.TempData
{
    public static class TempData
    {

        public static List<TempPlayer> Players { get; set; } = new List<TempPlayer>();

        public static TempPlayer CurrentPlayer { get; set; } = new TempPlayer();

        public static bool AuthPlayer(string username, string password)
        {
            var player = Players.FirstOrDefault(p => p.Username == username && p.Password == password);
            if (player == null)
            {
                return false;
            }

            return true;
        }

        public static bool RegisterPlayer(TempPlayer player)
        {
            if (Players.Any(p => p.Username == player.Username))
            {
                return false;
            }

            Players.Add(player);
            return true;
        }
    }
}
