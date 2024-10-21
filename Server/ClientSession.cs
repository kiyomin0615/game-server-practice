using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Server
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

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnect: {endPoint}");

            // Packet packet = new Packet() { size = 100, id = 1 };

            // ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            // byte[] bytes1 = BitConverter.GetBytes(packet.size);
            // byte[] bytes2 = BitConverter.GetBytes(packet.id);

            // Array.Copy(bytes1, 0, openSegment.Array, openSegment.Offset, bytes1.Length);
            // Array.Copy(bytes2, 0, openSegment.Array, openSegment.Offset + bytes1.Length, bytes2.Length);
            // ArraySegment<byte> sendBuffer = SendBufferHelper.Close(bytes1.Length + bytes2.Length);
            // Send(sendBuffer);

            Thread.Sleep(3000);

            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnect: {endPoint}");
        }

        public override void OnPacketReceived(ArraySegment<byte> buffer)
        {
            // Deserialization
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketId)id)
            {
                case PacketId.PlayerInfoRequest:
                    PlayerInfoRequest packet = new PlayerInfoRequest();
                    packet.Deserialize(buffer);
                    Console.WriteLine($"PlayerInfoRequest\nPlayer ID: {packet.playerId}");
                    break;
                case PacketId.PlayerInfoResponse:
                    break;
                default:
                    break;
            }

            Console.WriteLine($"Packet Received.\nPacket ID: {id}, Packet Size: {size}");
        }

        public override void OnSent(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }
    }
}