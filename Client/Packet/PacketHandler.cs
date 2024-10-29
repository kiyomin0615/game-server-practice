using Client;
using ServerCore;

public class PacketHandler
{
    public static void HandleS_ChatPacket(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;
    }
}