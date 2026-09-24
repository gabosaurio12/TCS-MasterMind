using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MasterMind_Client.TempData
{
    public class TempPlayer
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public ImageSource Avatar { get; set; }
        public bool IsOnline { get; set; }
    }
}
