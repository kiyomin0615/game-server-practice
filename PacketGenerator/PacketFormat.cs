using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PacketGenerator
{
    public class PacketFormat
    {
        /*
         * {0} : packet name
         * {1} : member variables
         * {2} : deserialize members
         * {3} : serialize members
         */
        public static string packetFormat =
@"class {0}
{{
    {1}
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
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketId.{0});
        count += sizeof(ushort);
        {3}
        success &= BitConverter.TryWriteBytes(span, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }}
}}";

        /*
         * {0} : member type
         * {1} : member name
         */
        public static string memberFormat =
@"public {0} {1};";

        /*
         * {0} : member name
         * {1} : deserialize method
         * {1} : member type
         */
        public static string deserializeFormat =
@"{0} = BitConverter.{1}(span.Slice(count, span.Length - count));
count += sizeof({1});";

        /*
         * {0} : member name
         */
        public static string deserializeStringFormat =
@"ushort {0}Length = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
this.playerName = Encoding.Unicode.GetString(span.Slice(count, {0}Length));
count += {0}Length;";

        /*
         * {0} : member name
         * {1} : member type
         */
        public static string serializeFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0});
count += sizeof({1});";

        /*
         * {0} : member name
         */
        public static string serializeStringFormat =
@"ushort {0}Length = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Length);
count += sizeof(ushort);
count += {0}Length;";
    }
}