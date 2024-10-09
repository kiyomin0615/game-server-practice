using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

namespace ServerCore
{
    public class Session
    {
        Socket sessionSocket;

        int isDisconnected = 0;

        public void Start(Socket sessionSocket)
        {
            this.sessionSocket = sessionSocket;

            SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            receiveArgs.SetBuffer(new byte[1024], 0, 1024);

            StartReceiving(receiveArgs);
        }

        #region Receive
        void StartReceiving(SocketAsyncEventArgs receiveArgs)
        {
            bool isPending = sessionSocket.ReceiveAsync(receiveArgs);
            if (!isPending)
            {
                OnReceiveCompleted(null, receiveArgs);
            }
        }

        void OnReceiveCompleted(object? sender, SocketAsyncEventArgs receiveArgs)
        {
            if (receiveArgs.BytesTransferred > 0 && receiveArgs.SocketError == SocketError.Success)
            {
                try
                {
                    string data = Encoding.UTF8.GetString(receiveArgs.Buffer, receiveArgs.Offset, receiveArgs.BytesTransferred);
                    Console.WriteLine($"[From Client] {data}");

                    StartReceiving(receiveArgs);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnReceiveCompleted Failed: {e}");
                }

            }
            else
            {
                Console.WriteLine(receiveArgs.SocketError.ToString());
            }
        }
        #endregion

        public void Send(byte[] sendBuffer)
        {
            sessionSocket.Send(sendBuffer); // 클라이언트로 데이터를 전송한다
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref isDisconnected, 1) == 1)
            {
                return;
            }

            sessionSocket.Shutdown(SocketShutdown.Both); // 클라이언트와 서버 사이의 데이터 송수신 종료
            sessionSocket.Close(); // 클라이언트의 연결 끊기
        }
    }
}