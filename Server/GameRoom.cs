using ServerCore;

namespace Server
{
    public class GameRoom
    {
        List<ClientSession> sessions = new List<ClientSession>();

        JobQueue jobQueue = new JobQueue();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        public void Enter(ClientSession session)
        {
            sessions.Add(session);
            session.GameRoom = this;
        }

        public void Exit(ClientSession session)
        {
            sessions.Remove(session);
            session.GameRoom = null;
        }

        public void BroadCast(ClientSession session, string chat)
        {
            S_Chat chatPacket = new S_Chat();

            chatPacket.playerId = session.SessionId;
            chatPacket.chat = chat;

            ArraySegment<byte> segment = chatPacket.Serialize();

            pendingList.Add(segment);
        }

        public void FlushPackets()
        {
            foreach (ClientSession s in sessions)
            {
                s.Send(pendingList);
            }

            Console.WriteLine($"{pendingList.Count} 패킷 송신.");

            pendingList.Clear();
        }

        public void PushAction(Action job)
        {
            jobQueue.Push(job);
        }

        public Action PopAction()
        {
            return jobQueue.Pop();
        }
    }
}