using System;
using System.Net;
using System.Text;
using ServerCore;

namespace Client
{
    public enum PacketID
    {
        PlayerInfoRequest = 1,
        Test = 2,
        
    }

    class PlayerInfoRequest
    {
        public byte byteTest;
        public long playerId;
        public string playerName;
        public struct Skill
        {
            public int id;
            public ushort level;
            public float duration;
            public void Deserialize(ReadOnlySpan<byte> span, ref ushort count)
            {
                id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
                count += sizeof(int);
                
                level = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
                count += sizeof(ushort);
                
                duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
                count += sizeof(float);
                
            }
            public bool Serialize(Span<byte> span, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
                count += sizeof(int);
                
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.level);
                count += sizeof(ushort);
                
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.duration);
                count += sizeof(float);
                
                return success;
            }
        }
        
        public List<Skill> skills = new List<Skill>();
        
        public void Deserialize(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort); // size
            count += sizeof(ushort); // id
            this.byteTest = (byte)segment.Array[segment.Offset + count];
            count += sizeof(byte);
            
            playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
            count += sizeof(long);
            
            ushort playerNameLength = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            this.playerName = Encoding.Unicode.GetString(span.Slice(count, playerNameLength));
            count += playerNameLength;
            
            this.skills.Clear();
            ushort skillLength = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < skillLength; i++)
            {
                Skill skill = new Skill();
                skill.Deserialize(span, ref count);
                skills.Add(skill);
            }
            
        }

        public ArraySegment<byte> Serialize()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.PlayerInfoRequest);
            count += sizeof(ushort);
            segment.Array[segment.Offset + count] = (byte)this.byteTest;
            count += sizeof(byte);
            
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
            count += sizeof(long);
            
            ushort playerNameLength = (ushort)Encoding.Unicode.GetBytes(this.playerName, 0, this.playerName.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), playerNameLength);
            count += sizeof(ushort);
            count += playerNameLength;
            
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            foreach (Skill skill in this.skills)
            {
                success &= skill.Serialize(span, ref count);
            }
            
            success &= BitConverter.TryWriteBytes(span, count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnect: {endPoint}");

            PlayerInfoRequest packet = new PlayerInfoRequest() { playerId = 1001, playerName = "kiyomin" };
            packet.skills.Add(new PlayerInfoRequest.Skill() { id = 101, level = 1, duration = 1.0f });
            packet.skills.Add(new PlayerInfoRequest.Skill() { id = 201, level = 2, duration = 2.0f });
            packet.skills.Add(new PlayerInfoRequest.Skill() { id = 301, level = 3, duration = 3.0f });
            packet.skills.Add(new PlayerInfoRequest.Skill() { id = 401, level = 4, duration = 4.0f });

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