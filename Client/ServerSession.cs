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

    public abstract class Packet
    {
        public ushort size;
        public ushort id;

        public abstract ArraySegment<byte> Serialize();
        public abstract void Deserialize(ArraySegment<byte> segment);
    }

    class PlayerInfoRequest : Packet
    {
        public long playerId;

        public PlayerInfoRequest()
        {
            this.id = (ushort)PacketId.PlayerInfoRequest;
        }

        public override void Deserialize(ArraySegment<byte> seg)
        {
            ushort count = 0;

            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);

            ushort size = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
            count += sizeof(ushort);
            ushort id = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
            count += sizeof(ushort);

            this.playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
            count += sizeof(long);
        }

        public override ArraySegment<byte> Serialize()
        {
            ArraySegment<byte> seg = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
            count += sizeof(long);
            success &= BitConverter.TryWriteBytes(span, count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

    class PlayerInfoResponse : Packet
    {
        public int hp;
        public int attack;

        public override void Deserialize(ArraySegment<byte> segment)
        {
            throw new NotImplementedException();
        }

        public override ArraySegment<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnect: {endPoint}");

            PlayerInfoRequest packet = new PlayerInfoRequest() { playerId = 1001 };

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