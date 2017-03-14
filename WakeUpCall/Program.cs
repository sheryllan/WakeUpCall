using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace WakeUpCall
{
    public class Program
    {
        private static LinkedList<WakeUpCallRequest> _wakeupCallList;
        private static Dictionary<int,LinkedListNode<WakeUpCallRequest>> _requestDict;
        public static void Main(string[] args)
        {
            
            var getNextCallTimer = new Timer
                                   {
                                       Enabled = true,
                                       Interval = 5000
                                   };
            getNextCallTimer.Elapsed += GetNextCallTimerOnElapsed;
            getNextCallTimer.Start();

            MockAPI.RequestQueue = new Queue<WakeUpCallRequest>();
            _wakeupCallList = new LinkedList<WakeUpCallRequest>();
            _requestDict = new Dictionary<int, LinkedListNode<WakeUpCallRequest>>();
            while (true)
            {
                Console.WriteLine("Room Number:");
                int room;
                if (!Int32.TryParse(Console.ReadLine(), out room))
                {
                    Console.WriteLine("!!! Invalid room number, please enter an integer.");
                    continue;
                }
                    
                Console.WriteLine("Wake-up Time(HH:mm:ss):");
                var inputTime = Console.ReadLine();
                if (string.IsNullOrEmpty(inputTime))
                {
                    MockAPI.RequestQueue.Enqueue(new WakeUpCallRequest(room));
                    Console.WriteLine("--> Room num [{0}] Cancel wakeup call", room);
                }
                else
                {
                    TimeSpan time;
                    while (!TimeSpan.TryParse(inputTime, out time))
                    {
                        Console.WriteLine("!!! Invalid wake-up time, please enter a datetime.");
                        inputTime = Console.ReadLine();
                    }

                    Console.WriteLine("--> Room num [{0}] call at: {1}", room, time);
                    DateTime date = time < MockAPI.getCurrenTime() ? DateTime.Today.AddDays(1) : DateTime.Today;
                    MockAPI.RequestQueue.Enqueue(new WakeUpCallRequest(room, date + time));
                }
            }

        }

       

        private static void GetNextCallTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var request = MockAPI.getNextCallRequest();

            if (request != null)
            {
                // when the request of the room is cancelled
                if (request.WakeUpTime == null)
                {
                    RemoveFromSortedWakeUpList(request);
                }
                // when the request of the room is not cancelled
                else
                {
                    RemoveFromSortedWakeUpList(request);
                    InsertIntoSortedWakeUpList(request);                   

                }
            }
            
            Alarm();

        }

        private static LinkedListNode<WakeUpCallRequest> InsertIntoSortedWakeUpList(WakeUpCallRequest request)
        {
            if (request.WakeUpTime == null)
                return null;
            DateTime dateTimeToInsert = request.WakeUpTime.Value;

            LinkedListNode<WakeUpCallRequest> nodeToInsert = null;
            if (Monitor.TryEnter(_wakeupCallList,5000))
            {
                try
                {
                    LinkedListNode<WakeUpCallRequest> node = _wakeupCallList.First;

                    // when the list is empty
                    if (node == null)
                    {
                        nodeToInsert = _wakeupCallList.AddFirst(request);
                    }
                    else // when the list is not empty
                    {
                        while (node != null)
                        {
                            if (node.Value.WakeUpTime.Value >= dateTimeToInsert)
                                break;
                            node = node.Next;
                        }
                        nodeToInsert = node == null ? _wakeupCallList.AddLast(request) : _wakeupCallList.AddBefore(node, request);
                    }
                    _requestDict[request.RoomNo] = nodeToInsert;
                }
                finally
                {
                    Monitor.Exit(_wakeupCallList);
                }
            }
            return nodeToInsert;
        }

        private static void RemoveFromSortedWakeUpList(WakeUpCallRequest request)
        {
            if (Monitor.TryEnter(_wakeupCallList, 5000))
            {
                try
                {
                    LinkedListNode<WakeUpCallRequest> node;
                    if (_requestDict.TryGetValue(request.RoomNo, out node))
                    {
                        _wakeupCallList.Remove(node);
                        _requestDict.Remove(request.RoomNo);
                    }
                }
                finally
                {
                    Monitor.Exit(_wakeupCallList);
                }

            }
        }

        private static void Alarm()
        {
            if (Monitor.TryEnter(_wakeupCallList))
            {
                try
                {
                    if (_wakeupCallList.First == null)
                        return;
                    WakeUpCallRequest firstCallRequest = _wakeupCallList.First.Value;
                    DateTime earliestCallTime = firstCallRequest.WakeUpTime.Value;
                    if (DateTime.Today + MockAPI.getCurrenTime() >= earliestCallTime)
                    {
                        MockAPI.sendAlarmCall(firstCallRequest.RoomNo);
                        RemoveFromSortedWakeUpList(firstCallRequest);
                    }

                }
                finally
                {
                    Monitor.Exit(_wakeupCallList);
                }
            }
            
        }

    }
}
