using System;
using System.Net;
using System.Text;
using ServerCore;

namespace Client
{
    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnect: {endPoint}");

            C_PlayerInfoRequest packet = new C_PlayerInfoRequest() { playerId = 1001, playerName = "kiyomin" };
            packet.skills.Add(new C_PlayerInfoRequest.Skill() { id = 101, level = 1, duration = 1.0f });
            packet.skills.Add(new C_PlayerInfoRequest.Skill() { id = 201, level = 2, duration = 2.0f });
            packet.skills.Add(new C_PlayerInfoRequest.Skill() { id = 301, level = 3, duration = 3.0f });
            packet.skills.Add(new C_PlayerInfoRequest.Skill() { id = 401, level = 4, duration = 4.0f });

            ArraySegment<byte> sendBuffer = packet.Serialize();

            if (sendBuffer != null)
                Send(sendBuffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnect: {endPoint}");
        }

        public override int OnReceived(ArraySegment<byte> buffer)
        {
            int hp = BitConverter.ToInt32(buffer.Array, buffer.Offset); // 첫 4바이트에서 HP
            int attack = BitConverter.ToInt32(buffer.Array, buffer.Offset + sizeof(int)); // 다음 4바이트에서 Attack

            Console.WriteLine($"[From Server] HP: {hp}, Attack: {attack}");

            return buffer.Count;
        }

        public override void OnSent(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }
    }
}