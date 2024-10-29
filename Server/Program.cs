using System.Net;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener listener = new Listener();
        public static GameRoom GameRoom = new GameRoom();

        static void FlushGameRoomPackets()
        {
            GameRoom.PushAction(() => GameRoom.FlushPackets());
            JobTimer.Instance.Push(FlushGameRoomPackets, 250);
        }

        static void Main(string[] args)
        {
            string hostName = Dns.GetHostName(); // 호스트 네임
            IPHostEntry ipInstance = Dns.GetHostEntry(hostName); // ip 관련 인스턴스
            IPAddress ip = ipInstance.AddressList[0]; // ip 주소
            IPEndPoint endPoint = new IPEndPoint(ip, 7777); // 엔드포인트

            listener.Init(endPoint, () =>
            {
                return SessionManager.Instance.GenerateSession();
            });
            Console.WriteLine("Listening...");

            JobTimer.Instance.Push(FlushGameRoomPackets);

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}