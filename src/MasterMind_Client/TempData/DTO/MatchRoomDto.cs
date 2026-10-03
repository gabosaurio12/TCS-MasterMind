using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MasterMind_Client.TempData.DTO
{
    public class MatchRoomDto
    {
        public int MatchRoomId { get; set; }
        [RegularExpression(@"\bTimeTrial\b|\bTries\b")]
        public string RoomName { get; set; }
        public string Gamemode { get; set; }
        public int PlayerOneId { get; set; }
        public int PlayerTwoId { get; set; }
        public string PrivateRoomCode { get; set; }

        [RegularExpression(@"\bEasy\b|\bNormal\b|\bHard\b|\bEnigma\b")]
        public string Difficulty { get; set; }

        [RegularExpression(@"\bPublic\b|\bPrivate\b")]
        public string Privacy { get; set; }
    }
}
