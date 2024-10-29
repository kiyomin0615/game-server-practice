namespace Server
{
    public class SessionManager
    {
        static SessionManager instance = new SessionManager();
        public static SessionManager Instance { get { return instance; } }

        object lockObject = new object();

        int sessionId = 0;
        Dictionary<int, ClientSession> sessionDict = new Dictionary<int, ClientSession>();

        public ClientSession GenerateSession()
        {
            lock (lockObject)
            {
                int sessionId = this.sessionId++;
                ClientSession session = new ClientSession();

                session.SessionId = sessionId;
                sessionDict.Add(sessionId, session);

                Console.WriteLine($"세션 생성: {sessionId}");

                return session;
            }
        }

        public ClientSession Find(int id)
        {
            lock (lockObject)
            {
                ClientSession session = null;
                sessionDict.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (lockObject)
            {
                sessionDict.Remove(session.SessionId);

                Console.WriteLine($"세션 삭제: {session.SessionId}");
            }
        }
    }
}