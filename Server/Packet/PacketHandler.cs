using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server;
using ServerCore;

public class PacketHandler
{
    public static void HandleC_ChatPacket(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        C_Chat chatPacket = packet as C_Chat;

        if (clientSession.GameRoom == null)
            return;

        GameRoom room = clientSession.GameRoom;
        clientSession.GameRoom.PushAction(() => { room.BroadCast(clientSession, chatPacket.chat); });
    }
}