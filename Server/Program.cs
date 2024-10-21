using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener listener = new Listener();

        static void Main(string[] args)
        {
            string hostName = Dns.GetHostName(); // 호스트 네임
            IPHostEntry ipInstance = Dns.GetHostEntry(hostName); // ip 관련 인스턴스
            IPAddress ip = ipInstance.AddressList[0]; // ip 주소
            IPEndPoint endPoint = new IPEndPoint(ip, 7777); // 엔드포인트

            listener.Init(endPoint, () =>
            {
                return new ClientSession();
            });
            Console.WriteLine("Listening...");

            while (true)
            {
                /*
                    기존의 서버 코드는 메인 스레드에서 아무 작업도 수행하지 않고 있었습니다.
                    C# 콘솔 애플리케이션에서 메인 스레드가 아무 작업도 하지 않으면, 가비지 컬렉션이나 다른 백그라운드 작업들이 제대로 실행되지 않을 수 있습니다.
                */
                Console.WriteLine("Server is running...");
                Thread.Sleep(2000);
            }
        }
    }
}