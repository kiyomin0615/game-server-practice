using System.Net.Sockets;
using System.Net;

namespace ServerCore
{
    abstract public class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        public sealed override int OnReceived(ArraySegment<byte> buffer)
        {
            int processLength = 0;
            int packetCount = 0;

            while (true)
            {
                if (buffer.Count < HeaderSize)
                    break;

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                OnPacketReceived(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                packetCount++;

                processLength += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            if (packetCount > 1)
                Console.WriteLine($"{packetCount} 패킷 수신.");

            return processLength;
        }

        abstract public void OnPacketReceived(ArraySegment<byte> buffer);

    }

    abstract public class Session
    {
        Socket sessionSocket;

        ReceiveBuffer receiveBuffer = new ReceiveBuffer(65535);

        object lockObject = new object();

        Queue<ArraySegment<byte>> sendQueue = new Queue<ArraySegment<byte>>();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisconnected(EndPoint endPoint);
        public abstract int OnReceived(ArraySegment<byte> buffers);
        public abstract void OnSent(int numOfBytes);

        int isDisconnected = 0;

        void Clear()
        {
            lock (lockObject)
            {
                sendQueue.Clear();
                pendingList.Clear();
            }
        }

        public void Start(Socket sessionSocket)
        {
            this.sessionSocket = sessionSocket;

            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            StartReceiving();
        }

        #region Receive
        void StartReceiving()
        {
            receiveBuffer.Clear();

            ArraySegment<byte> writeSegment = receiveBuffer.writeSegment;
            receiveArgs.SetBuffer(writeSegment.Array, writeSegment.Offset, writeSegment.Count);

            try
            {
                bool isPending = sessionSocket.ReceiveAsync(receiveArgs);
                if (!isPending)
                {
                    OnReceiveCompleted(null, receiveArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"StartReceiving Failed : {e}");
            }
        }

        void OnReceiveCompleted(object? sender, SocketAsyncEventArgs receiveArgs)
        {
            if (receiveArgs.BytesTransferred > 0 && receiveArgs.SocketError == SocketError.Success)
            {
                try
                {
                    if (receiveBuffer.OnWrite(receiveArgs.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    int processLength = OnReceived(receiveBuffer.readSegment);
                    if (processLength < 0 || receiveBuffer.dataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    if (receiveBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

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
        public void Send(ArraySegment<byte> sendBuffer)
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

        public void Send(List<ArraySegment<byte>> sendBufferList)
        {
            if (sendBufferList.Count == 0)
                return;

            lock (lockObject)
            {
                foreach (ArraySegment<byte> sendBuffer in sendBufferList)
                {
                    sendQueue.Enqueue(sendBuffer);
                }

                if (pendingList.Count == 0)
                {
                    StartSending();
                }
            }
        }

        void StartSending()
        {
            if (isDisconnected == 1)
                return;

            while (sendQueue.Count > 0)
            {
                ArraySegment<byte> sendBuffer = this.sendQueue.Dequeue();
                pendingList.Add(sendBuffer);
            }
            sendArgs.BufferList = pendingList; // BufferList.Add()를 사용하지 말고 직접 할당해라

            try
            {
                bool isPending = sessionSocket.SendAsync(sendArgs);
                if (!isPending)
                {
                    OnSendCompleted(null, sendArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"StartSending Failed : {e}");
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
                return;

            OnDisconnected(sessionSocket.RemoteEndPoint);

            sessionSocket.Shutdown(SocketShutdown.Both); // 클라이언트와 서버 사이의 데이터 송수신 종료
            sessionSocket.Close(); // 클라이언트의 연결 끊기

            Clear();
        }
    }
}