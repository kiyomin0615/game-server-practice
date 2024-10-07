using System;
using System.Threading;

namespace ServerCore
{
  class Program
  {
    static void Task1(object state)
    {
      for (int i = 0; i < 10; i++)
        Console.WriteLine($"Tasks are being done by thread pool: {i + 1}");
    }

    static void Task2(object state)
    {
      for (int i = 0; i < 10; i++)
        Console.WriteLine($"Tasks are being done by thread: {i + 1}");
    }

    // 메인 쓰레드
    static void Main(string[] args)
    {
      Console.WriteLine("Main Thread Start!");

      // Case 1
      Thread thread = new Thread(Task2); // 쓰레드 생성해서 태스크 수행(직접 고용)
      thread.Name = "Test Thread";
      thread.IsBackground = true;

      thread.Start(); // 쓰레드 시작
      thread.Join(); // 쓰레드가 끝날 떄까지 대기

      // Case 2
      ThreadPool.SetMinThreads(1, 1);
      ThreadPool.SetMaxThreads(5, 5);
      for (int i = 0; i < 5; i++)
        ThreadPool.QueueUserWorkItem(Task1); // 쓰레드 풀에 태스크 전달(파견 회사)

      // Case 3
      // Task 클래스는 스레드 풀을 사용하되, LongRunning 옵션을 통해 별도의 스레드를 사용함
      Task task = new Task(Task1, TaskCreationOptions.LongRunning);
      task.Start();

      Console.WriteLine("Main Thread End!");
    }
  }
}