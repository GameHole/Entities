using System;
using System.Text;
using System.Collections.Concurrent;
namespace Entities.Net
{
    public class ByteStream :IByteStream
    {
        private static readonly ConcurrentStack<ByteStream> stack = new ConcurrentStack<ByteStream>();
        public static ByteStream Take()
        {
            ByteStream stream;
            if(!stack.TryPop(out stream))
            {
                stream = new ByteStream();
            }
            return stream;
        }
        public static void Return(ByteStream stream)
        {
            stream.Destroy();
            stack.Push(stream);
        }
        public ByteStream(byte[] buffer, int startIndex, int lenght)
        {
            Set(buffer, startIndex, lenght);
        }
        private ByteStream() { }
        public byte[] Buffer { get; private set; }
        public int index { get; private set; }
        public void Set(byte[] buffer, int startIndex, int lenght)
        {
            index = 0;
            Buffer = unSafeByteHelper.Take(lenght);
            Array.Copy(buffer, startIndex, Buffer, 0, lenght);
        }
        public void Set(byte[] buffer)
        {
            index = 0;
            this.Buffer = buffer;
        }
        public bool GetBool(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(bool));
            if (idx == -1) return false;
            return BitConverter.ToBoolean(Buffer, idx);
        }

        public byte GetByte(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(byte));
            if (idx == -1) return 0;
            return Buffer[idx];
        }
        public double GetDouble(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(double));
            if (idx == -1) return 0;
            return BitConverter.ToDouble(Buffer, idx);
        }

        public float GetFloat(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(float));
            if (idx == -1) return 0;
            return BitConverter.ToSingle(Buffer, idx);
        }

        public int GetInt(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(int));
            if (idx == -1) return 0;
            return BitConverter.ToInt32(Buffer, idx);
        }

        public long GetLong(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(long));
            if (idx == -1) return 0;
            return BitConverter.ToInt64(Buffer, idx);
        }
        public short GetShort(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(short));
            if (idx == -1) return 0;
            return BitConverter.ToInt16(Buffer, idx);
        }

        public uint GetUInt(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(uint));
            if (idx == -1) return 0;
            return BitConverter.ToUInt32(Buffer, idx);
        }

        public ulong GetULong(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(ulong));
            if (idx == -1) return 0;
            return BitConverter.ToUInt64(Buffer, idx);
        }

        public ushort GetUShort(bool peek = false)
        {
            int idx = GetIndex(peek, sizeof(ushort));
            if (idx == -1) return 0;
            return BitConverter.ToUInt16(Buffer, idx);
        }

        public void MoveIndex(int length)
        {
            index += length;
        }
        public bool HasMore()
        {
            return index <= Buffer.Length - 1;
        }
        internal int GetIndex(bool peek, int size)
        {
            int idx = -1;
            if (Enough(size))
            {
                idx = index;
                _MoveIndex(peek, size);
            }
            else
            {
                throw new ByteStreamIndexOutOfRangeException(
                    $"Converting Value Type Error ,In Stream, " +
                    $"Last Lenght = {Buffer.Length - index} ,Need Lenght = {size}");
            }
            return idx;
        }
        //public string GetStringToEnd(bool peek = false)
        //{
        //    return _GetString(_buffer.Length - index,peek);
        //}
        public string GetString(bool peek = false)
        {
            int len = GetUShort();
            return _GetString(len);
        }
        private string _GetString(int length, bool peek = false)
        {
            int idx = GetIndex(peek, length);
            if (idx == -1) return string.Empty;
            return Encoding.Default.GetString(Buffer, idx, length);
        }
        private void _MoveIndex(bool peek,int lenght)
        {
            if (!peek)
            {
                MoveIndex(lenght);
            }
        }

        public bool Enough(int count)
        {
            return !(index + count > Buffer.Length);
        }

        private void Destroy()
        {
            index = 0;
            if (this.Buffer == null) return;
            unSafeByteHelper.Return(this.Buffer);
            this.Buffer = null;
        }
    }

    [Serializable]
    public class ByteStreamIndexOutOfRangeException : Exception
    {
        public ByteStreamIndexOutOfRangeException() { }
        public ByteStreamIndexOutOfRangeException(string message) : base(message) { }
        public ByteStreamIndexOutOfRangeException(string message, Exception inner) : base(message, inner) { }
        protected ByteStreamIndexOutOfRangeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
