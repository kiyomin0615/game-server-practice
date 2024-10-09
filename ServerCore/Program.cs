using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener listener = new Listener();

        static void OnAcceptSucceeded(Socket clientSocket)
        {
            try
            {
                // Receive
                byte[] receiveBuffer = new byte[1024];
                int size = clientSocket.Receive(receiveBuffer); // 클라이언트로부터 데이터를 전송받는다
                string data = Encoding.UTF8.GetString(receiveBuffer, 0, size);
                Console.WriteLine($"[From Client] {data}");

                // Send
                byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to game server!");
                clientSocket.Send(sendBuffer); // 클라이언트로 데이터를 전송한다

                // Disconnect
                clientSocket.Shutdown(SocketShutdown.Both); // 클라이언트와 서버 사이의 데이터 송수신 종료
                clientSocket.Close(); // 클라이언트의 연결 끊기
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