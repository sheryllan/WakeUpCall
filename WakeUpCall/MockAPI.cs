using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakeUpCall
{
    public class MockAPI
    {
        public static Queue<WakeUpCallRequest> RequestQueue { get; set; }

        public static WakeUpCallRequest getNextCallRequest()
        {
            if(RequestQueue.Count > 0)
                return RequestQueue.Dequeue();
            return null;
        }

        public static void sendAlarmCall(int room)
        {
            Console.WriteLine();
            Console.WriteLine("****** Room:{0} Time:{1} Wake Uppppppppppp *********", room, DateTime.Now);
            Console.WriteLine();
        }

        public static TimeSpan getCurrenTime()
        {
            return DateTime.Now.TimeOfDay;
        }
    }
}
