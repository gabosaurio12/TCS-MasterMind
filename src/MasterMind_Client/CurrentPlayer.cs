using MasterMind_Client.PlayerService;
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
        public string AvatarPath { get; private set; }

        private CurrentPlayer()
        {
        }

        public void SetCurrentPlayer(Player player)
        {
            Id = player.PlayerId;
            Username = player.Username;
            Email = player.Email;
            if (AvatarPath != player.AvatarPath)
            {
                Avatar = null;
            }

            AvatarPath = player.AvatarPath;
        }

        public void SetAvatar(ImageSource avatar, string avatarPath)
        {
            Avatar = avatar;
            AvatarPath = avatarPath;
        }
    }
}
