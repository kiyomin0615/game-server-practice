using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener listener = new Listener();

        static void OnAcceptSucceeded(Socket sessionSocket)
        {
            try
            {
                Session session = new Session();
                session.Start(sessionSocket);

                byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to game server!");
                session.Send(sendBuffer);

                Thread.Sleep(1000);

                session.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        static void Main(string[] args)
        {
            string hostName = Dns.GetHostName(); // 호스트 네임
            IPHostEntry ipInstance = Dns.GetHostEntry(hostName); // ip 관련 인스턴스
            IPAddress ip = ipInstance.AddressList[0]; // ip 주소
            IPEndPoint endPoint = new IPEndPoint(ip, 7777); // 엔드포인트

            listener.Init(endPoint, OnAcceptSucceeded);
            Console.WriteLine("Listening...");

            while (true)
            {
                //
            }
        }
    }
}