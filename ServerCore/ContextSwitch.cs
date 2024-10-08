using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore
{
    // Context Switch(문맥 교환)
    // CPU가 현재 쓰레드 또는 프로세스의 실행을 일시적으로 중단하고, 다른 쓰레드 또는 프로세스를 실행하는 것
    public class ContextSwitch
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

                // 쓰레드 실행 일시 중단
                Thread.Sleep(1000); // 시간 동안 현재 쓰레드의 실행을 중단한다
                Thread.Sleep(0); // 우선순위가 높은 쓰레드에게 CPU를 양보한다
                Thread.Yield(); // 다른 쓰레드에게 CPU를 양보한다
            }
        }

        public void Release()
        {
            isLocked = 0;
        }
    }
}