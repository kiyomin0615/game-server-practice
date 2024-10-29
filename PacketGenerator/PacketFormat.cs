using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PacketGenerator
{
    public class PacketFormat
    {
        public static string managerFormat =
@"using ServerCore;

public class PacketManager
{{
    // Singleton Pattern
    static PacketManager instance;
    public static PacketManager Instance
    {{
        get
        {{
            if (instance == null)
                instance = new PacketManager();
            return instance;
        }}
    }}

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> deserializerDict = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> packetHandlerDict = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
        {0}
    }}

    public void ProcessPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (deserializerDict.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }}

    void DeserializePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T packet = new T();
        packet.Deserialize(buffer);

        Action<PacketSession, IPacket> action = null;
        if (packetHandlerDict.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }}
}}
";

        public static string managerRegisterFormat = 
@"
        deserializerDict.Add((ushort)PacketID.{0}, DeserializePacket<{0}>);
        packetHandlerDict.Add((ushort)PacketID.{0}, PacketHandler.Handle{0}Packet);";

        public static string fileFormat =
@"using System;
using System.Net;
using System.Text;
using ServerCore;

public enum PacketID
{{
    {0}
}}

public interface IPacket
{{
	ushort Protocol {{ get; }}
	void Deserialize(ArraySegment<byte> segment);
	ArraySegment<byte> Serialize();
}}

{1}
";

        /*
         * {0} : packet name
         * {1} : packet number
         */
        public static string packetEnumFormat =
@"{0} = {1},";

        /*
         * {0} : packet name
         * {1} : member variables
         * {2} : deserialize members
         * {3} : serialize members
         */
        public static string packetFormat =
@"class {0} : IPacket
{{
    {1}
    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}

    public void Deserialize(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort); // size
        count += sizeof(ushort); // id
        {2}
    }}

    public ArraySegment<byte> Serialize()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);
        {3}
        success &= BitConverter.TryWriteBytes(span, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }}
}}
";

        /*
         * {0} : member type
         * {1} : member name
         */
        public static string memberFormat =
@"public {0} {1};";

        /*
         * {0} : Item name
         * {1} : item name
         * {2} : member variables
         * {3} : deserialize members
         * {4} : serialize members
         */
        public static string memberListFormat =
@"public struct {0}
{{
    {2}
    public void Deserialize(ReadOnlySpan<byte> span, ref ushort count)
    {{
        {3}
    }}
    public bool Serialize(Span<byte> span, ref ushort count)
    {{
        bool success = true;
        {4}
        return success;
    }}
}}

public List<{0}> {1}s = new List<{0}>();
";

        /*
         * {0} : member name
         * {1} : deserialize method
         * {2} : member type
         */
        public static string deserializeFormat =
@"{0} = BitConverter.{1}(span.Slice(count, span.Length - count));
count += sizeof({2});
";

        /*
         * {0} : variable name
         * {1} : variable type
         */
        public static string deserializeByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});
";

        /*
         * {0} : member name
         */
        public static string deserializeStringFormat =
@"ushort {0}Length = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(span.Slice(count, {0}Length));
count += {0}Length;
";

        /*
         * {0} : Item name
         * {1} : item name
         */
        public static string deserializeListFormat =
@"this.{1}s.Clear();
ushort {1}Length = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Length; i++)
{{
    {0} {1} = new {0}();
    {1}.Deserialize(span, ref count);
    {1}s.Add({1});
}}
";

        /*
         * {0} : member name
         * {1} : member type
         */
        public static string serializeFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0});
count += sizeof({1});
";

        /*
         * {0} : variable name
         * {1} : variable type
         */
        public static string serializeByteFormat =
@"segment.Array[segment.Offset + count] = ({1})this.{0};
count += sizeof({1});
";

        /*
         * {0} : member name
         */
        public static string serializeStringFormat =
@"ushort {0}Length = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Length);
count += sizeof(ushort);
count += {0}Length;
";

        /*
         * {0} : Item name
         * {1} : item name
         */
        public static string serializeListFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort){1}s.Count);
count += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
{{
    success &= {1}.Serialize(span, ref count);
}}
";
    }
}