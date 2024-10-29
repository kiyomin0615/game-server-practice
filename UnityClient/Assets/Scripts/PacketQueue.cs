using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unity는 메인 쓰레드에서만 유니티 오브젝트 조작이 가능하다
// 따라서 패킷을 서브 쓰레드에서 처리하는 것이 아니라, 패킷 큐를 사용해 패킷을 모아 메인 쓰레드에서 처리한다
public class PacketQueue
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<IPacket> packetQueue = new Queue<IPacket>();

    object lockObject = new object();

    public void Push(IPacket packet)
    {
        lock (lockObject)
        {
            packetQueue.Enqueue(packet);
        }
    }

    public IPacket Pop()
    {
        lock (lockObject)
        {
            if (packetQueue.Count == 0)
                return null;
            
            return packetQueue.Dequeue();
        }
    }
}
