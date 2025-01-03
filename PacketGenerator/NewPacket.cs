using System;
using System.Net;
using System.Text;
using ServerCore;

public enum PacketID
{
    C_PlayerInfoRequest = 1,
	Test = 2,
	
}

class C_PlayerInfoRequest
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
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.C_PlayerInfoRequest);
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
class Test
{
    public int intTest;
    public void Deserialize(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort); // size
        count += sizeof(ushort); // id
        intTest = BitConverter.ToInt32(span.Slice(count, span.Length - count));
		count += sizeof(int);
		
    }

    public ArraySegment<byte> Serialize()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.Test);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.intTest);
		count += sizeof(int);
		
        success &= BitConverter.TryWriteBytes(span, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }
}

