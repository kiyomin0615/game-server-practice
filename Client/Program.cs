using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

            try
            {
                Socket client = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // 1. Connect
                client.Connect(endPoint); // 서버에 연결 시도
                Console.WriteLine($"Connectecd to server({client.RemoteEndPoint}).");

                // 2. Send
                byte[] sendBuffer = Encoding.UTF8.GetBytes("Hello Server!");
                client.Send(sendBuffer);

                // 3. Receive
                byte[] receiveBuffer = new byte[1024];
                int size = client.Receive(receiveBuffer);
                string data = Encoding.UTF8.GetString(receiveBuffer, 0, size);
                Console.WriteLine($"[From Server] {data}");

                // 4. Disconnect
                client.Shutdown(SocketShutdown.Both); // 클라이언트와 서버 사이의 데이터 송수신 종료
                client.Close(); // 서버의 연결 끊기
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}