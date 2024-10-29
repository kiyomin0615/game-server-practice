using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        Socket listenerSocket;
        Func<Session> sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 10, int backlog = 100)
        {
            this.listenerSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // TCP 프로토콜
            this.sessionFactory += sessionFactory;

            listenerSocket.Bind(endPoint);

            listenerSocket.Listen(backlog); // 최대 대기 가능한 클라이언트 숫자: backlog

            for (int i = 0; i < count; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs(); // 비동기 이벤트에 대한 정보를 갖는다
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnClientAcceptCompleted); // 비동기 이벤트가 완료되면 실행될 이벤트 핸들러 등록
                StartAcceptingClients(args);
            }
        }

        void StartAcceptingClients(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool isPending = listenerSocket.AcceptAsync(args);
            if (!isPending)
            {
                OnClientAcceptCompleted(null, args);
            }
        }

        void OnClientAcceptCompleted(object? sender, SocketAsyncEventArgs args)
        {
            // 비동기 이벤트가 성공이라면
            if (args.SocketError == SocketError.Success)
            {
                Session session = sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            // 비동기 이벤트가 실패라면
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            StartAcceptingClients(args);
        }
    }
}