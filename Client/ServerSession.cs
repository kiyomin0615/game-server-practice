using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Client
{
    public enum PacketId
    {
        PlayerInfoRequest = 1,
        PlayerInfoResponse = 2
    }

    class Packet
    {
        public ushort size;
        public ushort id;
    }

    class PlayerInfoRequest : Packet
    {
        public long playerId;
    }

    class PlayerInfoResponse : Packet
    {
        public int hp;
        public int attack;
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnect: {endPoint}");

            PlayerInfoRequest packet = new PlayerInfoRequest() { id = (ushort)PacketId.PlayerInfoRequest, playerId = 1001 };

            for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> seg = SendBufferHelper.Open(4096);

                // Serialization
                ushort count = 0;
                bool success = true;

                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), packet.id);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), packet.playerId);
                count += 8;
                success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset, seg.Count), count);

                ArraySegment<byte> sendBuffer = SendBufferHelper.Close(count);

                if (success)
                    Send(sendBuffer);
            }
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