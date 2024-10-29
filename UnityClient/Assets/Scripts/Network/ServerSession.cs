using System;
using System.Net;
using ServerCore;

namespace Client
{
    public class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"서버({endPoint})와 연결 성공.");

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"서버({endPoint})와 연결 종료.");
        }

        public override void OnPacketReceived(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.ProcessPacket(this, buffer, (PacketSession session, IPacket packet) => { PacketQueue.Instance.Push(packet); });
        }

        public override void OnSent(int numOfBytes)
        {

        }
    }
}