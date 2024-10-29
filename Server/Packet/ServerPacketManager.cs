using System;
using System.Collections.Generic;
using ServerCore;

public class PacketManager
{
    // Singleton Pattern
    static PacketManager instance = new PacketManager();
    public static PacketManager Instance { get { return instance; } }

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> deserializerDict = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> packetHandlerDict = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public PacketManager()
    {
        Register();
    }

    public void Register()
    {
        
        deserializerDict.Add((ushort)PacketID.C_Chat, DeserializePacket<C_Chat>);
        packetHandlerDict.Add((ushort)PacketID.C_Chat, PacketHandler.HandleC_ChatPacket);

    }

    public void ProcessPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> callback = null)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (deserializerDict.TryGetValue(id, out func))
        {
            IPacket packet = func.Invoke(session, buffer);
            if (callback != null)
                callback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }
    }

    T DeserializePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T packet = new T();
        packet.Deserialize(buffer);

        return packet;
    }

    public void HandlePacket(PacketSession session, IPacket packet)
    {
        Action<PacketSession, IPacket> action = null;
        if (packetHandlerDict.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }
}
