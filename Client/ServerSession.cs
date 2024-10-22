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
        public string playerName;

        public PlayerInfoRequest()
        {
            this.id = (ushort)PacketId.PlayerInfoRequest;
        }

        public override void Deserialize(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort); // size
            count += sizeof(ushort); // id

            this.playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
            count += sizeof(long);

            ushort playerNameLength = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            this.playerName = Encoding.Unicode.GetString(span.Slice(count, playerNameLength));
        }

        public override ArraySegment<byte> Serialize()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
            count += sizeof(long);

            // Case 01
            ushort playerNameLength = (ushort)Encoding.Unicode.GetByteCount(this.playerName); // UTF-16
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), playerNameLength);
            count += sizeof(ushort);
            Array.Copy(Encoding.Unicode.GetBytes(this.playerName), 0, segment.Array, count, playerNameLength);
            count += playerNameLength;

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

            PlayerInfoRequest packet = new PlayerInfoRequest() { playerId = 1001, playerName = "kiyomin" };

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