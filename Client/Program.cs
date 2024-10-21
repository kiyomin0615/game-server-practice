using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string hostName = Dns.GetHostName(); // 호스트 네임
            IPHostEntry ipInstance = Dns.GetHostEntry(hostName); // ip 관련 인스턴스
            IPAddress ip = ipInstance.AddressList[0]; // ip 주소
            IPEndPoint endPoint = new IPEndPoint(ip, 7777); // 엔드포인트

            Connector connector = new Connector();
            connector.Connect(endPoint, () =>
            {
                return new ServerSession();
            });

            while (true)
            {
                try
                {
                    Console.WriteLine("Client is running...");
                    Thread.Sleep(2000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}