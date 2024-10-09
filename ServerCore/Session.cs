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

        object lockObject = new object();

        bool isPending = false;
        Queue<byte[]> sendQueue = new Queue<byte[]>();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();

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
                    string data = Encoding.UTF8.GetString(receiveArgs.Buffer, receiveArgs.Offset, receiveArgs.BytesTransferred);
                    Console.WriteLine($"[From Client] {data}");

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
                if (!this.isPending)
                {
                    StartSending();
                }
            }

        }

        void StartSending()
        {
            this.isPending = true;

            byte[] sendBuffer = this.sendQueue.Dequeue();
            sendArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);

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
                        if (sendQueue.Count > 0)
                        {
                            StartSending();
                        }
                        else
                        {
                            this.isPending = false;
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

            sessionSocket.Shutdown(SocketShutdown.Both); // 클라이언트와 서버 사이의 데이터 송수신 종료
            sessionSocket.Close(); // 클라이언트의 연결 끊기
        }
    }
}