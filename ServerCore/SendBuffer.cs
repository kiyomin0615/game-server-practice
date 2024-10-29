namespace ServerCore
{
    public class SendBufferHelper
    {
        // 서로 다른 세션 사이에서 버퍼는 공유되지만,
        // 서로 다른 쓰레드 사이에서 버퍼는 공유되지 않는다
        public static ThreadLocal<SendBuffer> currentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });
        public static int chunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (currentBuffer.Value == null)
                currentBuffer.Value = new SendBuffer(chunkSize);

            if (currentBuffer.Value.emptySize < reserveSize)
                currentBuffer.Value = new SendBuffer(chunkSize);

            return currentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return currentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        byte[] buffer;
        int usedSize = 0; // same as index of buffer
        public int emptySize { get { return buffer.Length - usedSize; } }

        public SendBuffer(int chunkSize)
        {
            buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > emptySize)
                return null;

            return new ArraySegment<byte>(buffer, usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, this.usedSize, usedSize);
            this.usedSize += usedSize;
            return segment;
        }
    }
}