using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Client
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnect: {endPoint}");

            for (int i = 0; i < 10; i++)
            {
                byte[] sendBuffer = Encoding.UTF8.GetBytes($"Hello Server! {i}\n");
                Send(sendBuffer);
            }
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnect: {endPoint}");
        }
        public override void OnReceived(ArraySegment<byte> buffer)
        {
            string data = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {data}");
        }
        public override void OnSent(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }
    }

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
                return new GameSession();
            });

            while (true)
            {
                try
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(1000);
            }
        }
    }
}