using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore
{
    // Spin Lock(스핀 락)은 락을 취득할 때까지 계속해서 기다린다
    public class SpinLock
    {
        volatile int isLocked = 0;

        public void Acquire()
        {
            while (true)
            {
                // if isLocked == 0, then isLocked = 1
                if (Interlocked.CompareExchange(ref isLocked, 1, 0) == 0)
                {
                    break;
                }
            }
        }

        public void Release()
        {
            isLocked = 0;
        }
    }
}