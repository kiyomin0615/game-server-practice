using ServerCore;

public class PacketManager
{
    // Singleton Pattern
    static PacketManager instance;
    public static PacketManager Instance
    {
        get
        {
            if (instance == null)
                instance = new PacketManager();
            return instance;
        }
    }

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> deserializerDict = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> packetHandlerDict = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        
        deserializerDict.Add((ushort)PacketID.S_Test, DeserializePacket<S_Test>);
        packetHandlerDict.Add((ushort)PacketID.S_Test, PacketHandler.HandleS_TestPacket);

    }

    public void ProcessPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (deserializerDict.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }

    void DeserializePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T packet = new T();
        packet.Deserialize(buffer);

        Action<PacketSession, IPacket> action = null;
        if (packetHandlerDict.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }
}
