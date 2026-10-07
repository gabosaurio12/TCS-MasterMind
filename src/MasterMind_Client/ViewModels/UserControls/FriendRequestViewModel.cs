using MasterMind_Client.Assets;
using MasterMind_Client.Data;
using MasterMind_Client.ViewModels.Base;
using System;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class FriendRequestViewModel : ViewModelBase
    {
        public FriendRequestViewModel(string username, Friendship request)
        {
            Username = username;
            Request = request;
            OnlineBrush = UtilsUI.OfflineGray;
            AcceptCommand = new RelayCommand(_ => Accepted?.Invoke(this, Request));
            RejectCommand = new RelayCommand(_ => Rejected?.Invoke(this, Request));
        }

        public string Username { get; }

        public Brush OnlineBrush { get; }

        public Friendship Request { get; }

        public RelayCommand AcceptCommand { get; }

        public RelayCommand RejectCommand { get; }

        public event EventHandler<Friendship> Accepted;

        public event EventHandler<Friendship> Rejected;
    }
}
