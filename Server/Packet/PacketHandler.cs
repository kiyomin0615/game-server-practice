using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;

public class PacketHandler
{
    public static void HandleC_PlayerInfoRequestPacket(PacketSession session, IPacket p)
    {
        C_PlayerInfoRequest packet = p as C_PlayerInfoRequest;

        Console.WriteLine($"C_PlayerInfoRequest\nPlayer ID: {packet.playerId}, Player Name: {packet.playerName}\n");

        foreach (C_PlayerInfoRequest.Skill skill in packet.skills)
        {
            Console.WriteLine($"Skill ID: {skill.id}, Skill Level: {skill.level}, Skill Duration: {skill.duration}\n");
        }
    }
}