namespace Server
{
    public class GameRoom
    {
        object lockObject = new object();

        List<ClientSession> sessions = new List<ClientSession>();

        public void Enter(ClientSession session)
        {
            lock (lockObject)
            {
                sessions.Add(session);
                session.GameRoom = this;
            }
        }

        public void Exit(ClientSession session)
        {
            lock (lockObject)
            {
                sessions.Remove(session);
                session.GameRoom = null;
            }
        }

        public void BroadCast(ClientSession session, string chat)
        {
            S_Chat chatPacket = new S_Chat();

            chatPacket.playerId = session.SessionId;
            chatPacket.chat = chat;

            ArraySegment<byte> segment = chatPacket.Serialize();

            lock (lockObject)
            {
                foreach (ClientSession s in sessions)
                {
                    s.Send(segment);
                }
            }
        }
    }
}