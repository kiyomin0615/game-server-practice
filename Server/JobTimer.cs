using System;
using System.Collections.Generic;
using ServerCore;

namespace Server
{
    struct JobTimerElement : IComparable<JobTimerElement>
    {
        public int executionTick;
        public Action action;

        public int CompareTo(JobTimerElement other)
        {
            return other.executionTick - executionTick;
        }
    }

    public class JobTimer
    {
        PriorityQueue<JobTimerElement> pq = new PriorityQueue<JobTimerElement>();

        object lockObject = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tick = 0)
        {
            JobTimerElement el;
            el.executionTick = System.Environment.TickCount + tick;
            el.action = action;

            lock (lockObject)
            {
                pq.Push(el);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;

                JobTimerElement el;

                lock (lockObject)
                {
                    if (pq.Count == 0)
                        break;

                    el = pq.Peek();
                    if (el.executionTick > now)
                        break;

                    pq.Pop();
                }

                el.action.Invoke();
            }
        }
    }
}