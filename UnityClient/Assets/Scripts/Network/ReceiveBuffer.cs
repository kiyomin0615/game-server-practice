using System;

namespace ServerCore
{
    public class ReceiveBuffer
    {
        ArraySegment<byte> buffer;
        int readPos;
        int writePos;

        public int dataSize { get { return writePos - readPos; } }
        public int emptySize { get { return buffer.Count - writePos; } }

        public ReceiveBuffer(int bufferSize)
        {
            buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public ArraySegment<byte> readSegment
        {
            get { return new ArraySegment<byte>(buffer.Array, buffer.Offset + readPos, dataSize); }
        }

        public ArraySegment<byte> writeSegment
        {
            get { return new ArraySegment<byte>(buffer.Array, buffer.Offset + writePos, emptySize); }
        }

        public void Clear()
        {
            int dataSize = this.dataSize;

            if (dataSize == 0)
            {
                readPos = writePos = 0;
            }
            else
            {
                Array.Copy(buffer.Array, buffer.Offset + readPos, buffer.Array, buffer.Offset, dataSize);
                readPos = 0;
                writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > dataSize)
            {
                return false;
            }

            readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > emptySize)
            {
                return false;
            }

            writePos += numOfBytes;
            return true;
        }

    }
}