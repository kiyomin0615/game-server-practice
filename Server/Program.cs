﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Server
{
    class Player {
        public int hp;
        public int attack;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnect: {endPoint}");

            Player player = new Player() { hp = 100, attack = 10};

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] bytes1 = BitConverter.GetBytes(player.hp);
            byte[] bytes2 = BitConverter.GetBytes(player.attack);
            Array.Copy(bytes1, 0, openSegment.Array, openSegment.Offset, bytes1.Length);
            Array.Copy(bytes2, 0, openSegment.Array, openSegment.Offset + bytes1.Length, bytes2.Length);
            ArraySegment<byte> sendBuffer = SendBufferHelper.Close(bytes1.Length + bytes2.Length);
            Send(sendBuffer);

            Thread.Sleep(1000);

            Disconnect();
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnect: {endPoint}");
        }
        public override int OnReceived(ArraySegment<byte> buffer)
        {
            string data = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {data}");

            return buffer.Count;
        }
        public override void OnSent(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }
    }

    class Program
    {
        static Listener listener = new Listener();

        static void Main(string[] args)
        {
            string hostName = Dns.GetHostName(); // 호스트 네임
            IPHostEntry ipInstance = Dns.GetHostEntry(hostName); // ip 관련 인스턴스
            IPAddress ip = ipInstance.AddressList[0]; // ip 주소
            IPEndPoint endPoint = new IPEndPoint(ip, 7777); // 엔드포인트

            listener.Init(endPoint, () =>
            {
                return new GameSession();
            });
            Console.WriteLine("Listening...");

            while (true)
            {
                //
            }
        }
    }
}