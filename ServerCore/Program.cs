using System;
using System.Threading;

namespace ServerCore
{
  class Program
  {
    // 멀티 쓰레딩 환경에서 컴파일러의 코드 최적화를 막는다
    // volatile은 사용하지 않는 것을 권장한다
    volatile static bool stop = false;

    static void MainThread()
    {
      Console.WriteLine("Thread Start!");

      while (!stop)
      {
        Console.WriteLine("Thread is looping!");
      }

      Console.WriteLine("Thread End!");
    }

    static void Main(string[] args)
    {
      Console.WriteLine("Main Thread Start!");
      Task task = new Task(MainThread);
      task.Start();

      Thread.Sleep(1000);
      stop = true;
      task.Wait(); // 태스크(쓰레드) 종료될 때까지 대기

      Console.WriteLine("Main Thread End!");
    }
  }
}