using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class MatchRoomViewModel : ViewModelBase
    {
        public MatchRoomViewModel(Room room)
        {
            Room = room;
            RoomName = room.Name;
            IsPrivate = room.RoomPrivacy == "Private";
        }

        public Room Room { get; }

        public string RoomName { get; }

        public bool IsPrivate { get; }
    }
}
