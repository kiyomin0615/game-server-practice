using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        /*
          Race Condition(경합 조건)
          멀티 쓰레드가 하나의 자원(resource)에 접근하기 위해 경쟁하는 상황
        */
        /*
          Atomicity(원자성)
          어떤 작업이 더 이상 쪼갤 수 없는 단위로 실행되어, 중간에 다른 작업으로 인해 방해받지 않는 것
          어떤 작업이 원자성을 갖는다면, 그 작업은 완전히 실행되거나, 전혀 실행되지 않은 상태만을 갖는다(All or Nothing)
          Race Condition을 막기 위해 원자성이 필요하다
        */
        static int num = 0;
        static SpinLock spinLock = new SpinLock();
        static ContextSwitch contextSwitch = new ContextSwitch();

        static void Thread1()
        {
            for (int i = 0; i < 10000; i++)
            {
                contextSwitch.Acquire();
                num++;
                contextSwitch.Release();
            }
        }

        static void Thread2()
        {
            for (int i = 0; i < 10000; i++)
            {
                contextSwitch.Acquire();
                num--;
                contextSwitch.Release();
            }
        }

        static void Main(string[] args)
        {
            Task task1 = new Task(Thread1);
            Task task2 = new Task(Thread2);

            task1.Start();
            task2.Start();

            Task.WaitAll(task1, task2);

            Console.WriteLine(num);
        }
    }
}