using System.Net;
using ServerCore;

namespace Server
{
    public class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom GameRoom { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"클라이언트({endPoint})와 연결 성공.");

            Program.GameRoom.PushAction(() => { Program.GameRoom.Enter(this); });
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);

            if (GameRoom != null)
            {
                GameRoom room = GameRoom;
                Program.GameRoom.PushAction(() => { room.Exit(this); });
                GameRoom = null;
            }

            Console.WriteLine($"클라이언트({endPoint})와 연결 종료.");
        }

        public override void OnPacketReceived(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.ProcessPacket(this, buffer);
        }

        public override void OnSent(int numOfBytes)
        {

        }
    }
}