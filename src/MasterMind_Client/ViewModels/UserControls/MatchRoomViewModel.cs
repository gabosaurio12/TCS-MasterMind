using MasterMind_Client.Assets;
using MasterMind_Client.TempData.DTO;
using MasterMind_Client.ViewModels.Base;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class MatchRoomViewModel : ViewModelBase
    {
        public MatchRoomViewModel(MatchRoomDto room)
        {
            Room = room;
            RoomName = room.RoomName;
            IsPrivate = room.Privacy == "Private";
            PrivacyBrush = room.Privacy == "Private" ? UtilsUI.PrivateRed : UtilsUI.PublicBlue;
        }

        public MatchRoomDto Room { get; }

        public string RoomName { get; }

        public bool IsPrivate { get; }

        public Brush PrivacyBrush { get; }
    }
}
