using MasterMind_Client.TempData.DTO;
using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class MatchRoomViewModel : ViewModelBase
    {
        public MatchRoomViewModel(MatchRoomDto room)
        {
            Room = room;
            RoomName = room.RoomName;
            IsPrivate = room.Privacy == "Private";
        }

        public MatchRoomDto Room { get; }

        public string RoomName { get; }

        public bool IsPrivate { get; }
    }
}
