using ServerCore;

namespace Server
{
    public class GameRoom
    {
        List<ClientSession> sessions = new List<ClientSession>();

        JobQueue jobQueue = new JobQueue();

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

            foreach (ClientSession s in sessions)
            {
                s.Send(segment);
            }
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