using Client;
using ServerCore;
using UnityEngine;

public class PacketHandler
{
    public static void HandleS_ChatPacket(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        if (chatPacket.playerId == 1)
        {
            Debug.Log(chatPacket.chat);
            GameObject go = GameObject.Find("Player");
            if (go == null)
                Debug.Log("Player Not Found.");
            else
                Debug.Log("Player Found.");
        }
    }
}