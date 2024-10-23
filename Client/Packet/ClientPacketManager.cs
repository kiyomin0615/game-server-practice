using System;
using System.Collections.Generic;
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

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> dict = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> handlerDict = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {

        dict.Add((ushort)PacketID.S_Test, HandlePacket<S_Test>);
        handlerDict.Add((ushort)PacketID.S_Test, PacketHandler.HandleS_Test);

    }

    public void OnPacketReceived(PacketSession session, ArraySegment<byte> buffer)
    {
        // Deserialization
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (dict.TryGetValue(id, out action))
            action.Invoke(session, buffer); // call HandlePacket()
    }

    void HandlePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T packet = new T();
        packet.Deserialize(buffer);
        Action<PacketSession, IPacket> action = null;
        if (handlerDict.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet); // call HandlePlayerInfoRequest()
    }
}
