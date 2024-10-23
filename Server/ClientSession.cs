using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Server
{
    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnect: {endPoint}");

            // Packet packet = new Packet() { size = 100, id = 1 };

            // ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            // byte[] bytes1 = BitConverter.GetBytes(packet.size);
            // byte[] bytes2 = BitConverter.GetBytes(packet.id);

            // Array.Copy(bytes1, 0, openSegment.Array, openSegment.Offset, bytes1.Length);
            // Array.Copy(bytes2, 0, openSegment.Array, openSegment.Offset + bytes1.Length, bytes2.Length);
            // ArraySegment<byte> sendBuffer = SendBufferHelper.Close(bytes1.Length + bytes2.Length);
            // Send(sendBuffer);

            Thread.Sleep(3000);

            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnect: {endPoint}");
        }

        public override void OnPacketReceived(ArraySegment<byte> buffer)
        {
            // Deserialization
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoRequest:
                    PlayerInfoRequest packet = new PlayerInfoRequest();
                    packet.Deserialize(buffer);
                    Console.WriteLine($"PlayerInfoRequest\nPlayer ID: {packet.playerId}, Player Name: {packet.playerName}\n");

                    foreach (PlayerInfoRequest.Skill skill in packet.skills)
                    {
                        Console.WriteLine($"Skill ID: {skill.id}, Skill Level: {skill.level}, Skill Duration: {skill.duration}\n");
                    }

                    break;
                default:
                    break;
            }

            Console.WriteLine($"Packet Received.\nPacket ID: {id}, Packet Size: {size}");
        }

        public override void OnSent(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }
    }
}