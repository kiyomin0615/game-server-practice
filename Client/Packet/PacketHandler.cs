using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;

public class PacketHandler
{
    public static void HandlePlayerInfoRequest(PacketSession session, IPacket p)
    {
        PlayerInfoRequest packet = p as PlayerInfoRequest;

        Console.WriteLine($"PlayerInfoRequest\nPlayer ID: {packet.playerId}, Player Name: {packet.playerName}\n");

        foreach (PlayerInfoRequest.Skill skill in packet.skills)
        {
            Console.WriteLine($"Skill ID: {skill.id}, Skill Level: {skill.level}, Skill Duration: {skill.duration}\n");
        }
    }

    public static void HandleTest(PacketSession session, IPacket p)
    {
        // TODO
    }
}