using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMind_Client.TempData
{
    public class TempVerificationCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int PlayerId { get; set; }
    }
}
