using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        /*
          TLS(Thread Local Storage)
          쓰레드 사이에 공유되지 않는 저장 공간(스택과는 별개)
        */
        static ThreadLocal<string> threadName = new ThreadLocal<string>();

        static void WhoAmI() {
            threadName.Value = $"Thread ID: {Thread.CurrentThread.ManagedThreadId}";
            Thread.Sleep(1000);
            Console.WriteLine(threadName.Value);
        }

        static void Main(string[] args)
        {
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI);
        }
    }
}