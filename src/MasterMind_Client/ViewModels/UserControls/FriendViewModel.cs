using MasterMind_Client.Helpers;
using System;
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
            RemoveCommand = new RelayCommand(_ => Removed?.Invoke(this, EventArgs.Empty));
        }

        public string Username { get; }

        public Brush OnlineBrush { get; }

        public RelayCommand RemoveCommand { get; }

        public event EventHandler Removed;
    }
}
