using System;
using System.Threading;

namespace ServerCore
{
  class Program
  {
    static int x = 0;
    static int y = 0;
    static int r1 = 0;
    static int r2 = 0;

    /*
      CPU가 최적화를 위해 코드 실행 순서를 바꿀 수 있다
      멀티 쓰레드 환경에서는 문제가 된다
    */ 
    /*
      메모리 배리어를 통해서,
      1. CPU의 코드 실행 순서 변경을 막는다
      2. 멀티 쓰레드간의 데이터를 '동기화'해서 '가시성'을 해결한다
    */
    static void Thread1() {
      y = 1; // Store y
      Thread.MemoryBarrier();
      r1 = x; // Load x
    }

    static void Thread2() {
      x = 1; // Store x
      Thread.MemoryBarrier();
      r2 = y; // Load y
    }

    static void Main(string[] args)
    {
      int count = 0;
      while (true) {
        count++;
        x = y = r1 = r2 = 0;

        Task task1 = new Task(Thread1);
        Task task2 = new Task(Thread2);

        task1.Start();
        task2.Start();

        Task.WaitAll(task1, task2);

        if (r1 == 0 && r2 == 0) {
          break;
        }
      }

      Console.WriteLine($"count: {count}");
    }
  }
}