using MasterMind_Client.TempData.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMind_Client.TempData
{
    public class TempFriendship
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public int AddreseeId { get; set; }
        public RequestStatusEnum Status { get; set; }
    }
}
