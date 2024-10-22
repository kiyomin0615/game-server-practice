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
        public string playerName;

        public struct Skill
        {
            public int id;
            public ushort level;
            public float duration;

            public bool Serialize(Span<byte> span, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), level);
                count += sizeof(ushort);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), duration);
                count += sizeof(float);

                return success;
            }

            public void Deserialize(ReadOnlySpan<byte> span, ref ushort count)
            {
                id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
                count += sizeof(int);
                level = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
                count += sizeof(ushort);
                duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
                count += sizeof(float);
            }
        }

        public List<Skill> skills = new List<Skill>();

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
            count += playerNameLength;

            skills.Clear();
            ushort skillLength = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < skillLength; i++)
            {
                Skill skill = new Skill();
                skill.Deserialize(span, ref count);
                skills.Add(skill);
            }
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

            ushort playerNameLength = (ushort)Encoding.Unicode.GetBytes(this.playerName, 0, this.playerName.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), playerNameLength);
            count += sizeof(ushort);
            count += playerNameLength;

            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            foreach (Skill skill in skills)
            {
                success &= skill.Serialize(span, ref count);
            }

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
                    Console.WriteLine($"PlayerInfoRequest\nPlayer ID: {packet.playerId}, Player Name: {packet.playerName}\n");

                    foreach (PlayerInfoRequest.Skill skill in packet.skills)
                    {
                        Console.WriteLine($"Skill ID: {skill.id}, Skill Level: {skill.level}, Skill Duration: {skill.duration}\n");
                    }

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