namespace Client
{
    public class SessionManager
    {
        static SessionManager instance = new SessionManager();
        public static SessionManager Instance { get { return instance; } }

        object lockObject = new object();

        List<ServerSession> sessions = new List<ServerSession>();

        public ServerSession GenerateSession()
        {
            lock (lockObject)
            {
                ServerSession session = new ServerSession();
                sessions.Add(session);
                return session;
            }
        }

        public void SendChatFromAllSessions()
        {
            lock (lockObject)
            {
                foreach (ServerSession s in sessions)
                {
                    C_Chat chatPacket = new C_Chat();
                    chatPacket.chat = $"Hello Server!";

                    ArraySegment<byte> segment = chatPacket.Serialize();

                    s.Send(segment);
                }
            }
        }
    }
}