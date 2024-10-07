using System;
using System.Threading;

namespace ServerCore
{
  class Program
  {
    static void Main(string[] args)
    {
      int[,] arr = new int[10000, 10000];

      // with cache
      {
        long start = DateTime.Now.Ticks;
        for (int y = 0; y < 10000; y++)
        {
          for (int x = 0; x < 10000; x++)
          {
            arr[y, x] = 1;
          }
        }
        long end = DateTime.Now.Ticks;
        Console.WriteLine($"It took {end - start}"); // 2769700
      }

      // without cache
      {
        long start = DateTime.Now.Ticks;
        for (int y = 0; y < 10000; y++)
        {
          for (int x = 0; x < 10000; x++)
          {
            arr[x, y] = 1;
          }
        }
        long end = DateTime.Now.Ticks;
        Console.WriteLine($"It took {end - start}"); // 7151980
      }
    }
  }
}