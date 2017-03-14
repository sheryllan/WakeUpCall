using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakeUpCall
{
    public class WakeUpCallRequest
    {
        public int RoomNo { get; set; }
        public DateTime? WakeUpTime { get; set; }

        public WakeUpCallRequest(int room, DateTime? time = null)
        {
            RoomNo = room;
            WakeUpTime = time;
        }

      

    }
}
