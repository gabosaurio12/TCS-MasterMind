using MasterMind_Client.Assets;
using System.Windows.Media;
using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class FriendViewModel : ViewModelBase
    {
        public FriendViewModel(string username)
        {
            Username = username;
            OnlineBrush = UtilsUI.OfflineGray;
        }

        public string Username { get; }

        public Brush OnlineBrush { get; }
    }
}
