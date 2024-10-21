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

            PlayerInfoRequest packet = new PlayerInfoRequest() { size = 4, id = (ushort)PacketId.PlayerInfoRequest, playerId = 1001 };

            // for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> seg = SendBufferHelper.Open(4096);

                // Serialization
                byte[] size = BitConverter.GetBytes(packet.size);
                byte[] id = BitConverter.GetBytes(packet.id);
                byte[] playerId = BitConverter.GetBytes(packet.playerId);

                ushort count = 0;
                Array.Copy(size, 0, seg.Array, seg.Offset + count, size.Length);
                count += (ushort)size.Length;
                Array.Copy(id, 0, seg.Array, seg.Offset + count, id.Length);
                count += (ushort)id.Length;
                Array.Copy(playerId, 0, seg.Array, seg.Offset + count, playerId.Length);
                count += (ushort)playerId.Length;
                ArraySegment<byte> sendBuffer = SendBufferHelper.Close(count);

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