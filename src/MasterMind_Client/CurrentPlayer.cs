using MasterMind_Client.Data;
using System.Windows.Media;

namespace MasterMind_Client
{
    public sealed class CurrentPlayer
    {
        private static CurrentPlayer instance;
        public static CurrentPlayer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CurrentPlayer();
                }
                return instance;
            }
        }

        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public ImageSource Avatar { get; private set; }

        private CurrentPlayer()
        {
        }

        public void SetCurrentPlayer(Player player)
        {
            Id = player.player_id;
            Username = player.username;
            Email = player.email;
        }
    }
}
