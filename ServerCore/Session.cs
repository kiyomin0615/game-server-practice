using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Net;

namespace ServerCore
{
    abstract public class Session
    {
        Socket sessionSocket;

        object lockObject = new object();

        Queue<byte[]> sendQueue = new Queue<byte[]>();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisconnected(EndPoint endPoint);
        public abstract void OnReceived(ArraySegment<byte> buffers);
        public abstract void OnSent(int numOfBytes);

        int isDisconnected = 0;

        public void Start(Socket sessionSocket)
        {
            this.sessionSocket = sessionSocket;

            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            receiveArgs.SetBuffer(new byte[1024], 0, 1024);

            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            StartReceiving();
        }

        #region Receive
        void StartReceiving()
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
                    OnReceived(new ArraySegment<byte>(receiveArgs.Buffer, receiveArgs.Offset, receiveArgs.BytesTransferred));
                    StartReceiving();
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

        #region Send
        public void Send(byte[] sendBuffer)
        {
            lock (lockObject)
            {
                sendQueue.Enqueue(sendBuffer);
                if (pendingList.Count == 0)
                {
                    StartSending();
                }
            }

        }

        void StartSending()
        {
            while (sendQueue.Count > 0)
            {
                byte[] sendBuffer = this.sendQueue.Dequeue();
                pendingList.Add(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length));
            }
            sendArgs.BufferList = pendingList; // BufferList.Add()를 사용하지 말고 직접 할당해라

            bool isPending = sessionSocket.SendAsync(sendArgs);
            if (!isPending)
            {
                OnSendCompleted(null, sendArgs);
            }
        }

        void OnSendCompleted(object? sender, SocketAsyncEventArgs sendArgs)
        {
            lock (lockObject)
            {
                if (sendArgs.BytesTransferred > 0 && sendArgs.SocketError == SocketError.Success)
                {
                    try
                    {
                        sendArgs.BufferList = null;
                        pendingList.Clear();

                        OnSent(sendArgs.BytesTransferred);

                        if (sendQueue.Count > 0)
                        {
                            StartSending();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnReceiveCompleted Failed: {e}");
                    }
                }
                else
                {
                    Console.WriteLine(sendArgs.SocketError.ToString());
                }
            }

        }
        #endregion

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref isDisconnected, 1) == 1)
            {
                return;
            }

            OnDisconnected(sessionSocket.RemoteEndPoint);

            sessionSocket.Shutdown(SocketShutdown.Both); // 클라이언트와 서버 사이의 데이터 송수신 종료
            sessionSocket.Close(); // 클라이언트의 연결 끊기
        }
    }
}