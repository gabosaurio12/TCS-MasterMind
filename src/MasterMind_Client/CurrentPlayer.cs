using MasterMind_Client.TempData;
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

        public void SetCurrentPlayer(TempPlayer player)
        {
            Id = player.Id;
            Username = player.Username;
            Email = player.Email;
            Avatar = player.Avatar;
        }
    }
}
