using System;
using System.Text;
using System.Collections.Concurrent;
namespace Entities.Net
{
    public static class unSafeByteHelper
    {
        private readonly static ConcurrentDictionary<int, ConcurrentQueue<byte[]>> dictionary
            = new ConcurrentDictionary<int, ConcurrentQueue<byte[]>>()
            {
                [sizeof(int)]=new ConcurrentQueue<byte[]>()
            };

        internal static byte[] Take(int count)
        {
            var queue = GetQueue(count);
            byte[] data;
            if (!queue.TryDequeue(out data))
            {
                data = new byte[count];
            }
            return data;
        }
        private static ConcurrentQueue<byte[]> GetQueue(int count)
        {
            ConcurrentQueue<byte[]> queue;
            if (!dictionary.TryGetValue(count, out queue))
            {
                queue = new ConcurrentQueue<byte[]>();
                if (!dictionary.TryAdd(count, queue))
                {
                    DebugUtility.Error($"GetQueue::This count {count} Queue Allready Added");
                }
            }
            return queue;
        }
        internal static void Return(byte[] buffer)
        {
            var queue = GetQueue(buffer.Length);
            queue.Enqueue(buffer);
        }
        public unsafe static byte[] ToBytes(this bool v)
        {
            byte[] s = Take(1);
            s[0] = v ? (byte)1 : (byte)0;
            return s;
        }
        public static byte[] ToBytes(this char value)
        {
            return ToBytes((short)value);
        }
        public unsafe static byte[] ToBytes(this short v)
        {
            byte[] array = Take(2);
            fixed (byte* ptr = array)
            {
                *(short*)ptr = v;
            }
            return array;
        }
        public unsafe static byte[] ToBytes(this int v)
        {
            byte[] array = Take(4);
            fixed(byte* ptr = array)
            {
                *(int*)ptr = v;
            }
            return array;
        }
        public unsafe static byte[] ToBytes(this long value)
        {
            byte[] array = Take(8);
            fixed (byte* ptr = array)
            {
                *(long*)ptr = value;
            }
            return array;
        }
        public static byte[] ToBytes(this ushort value)
        {
            return ToBytes((short)value);
        }
        public static byte[] ToBytes(this uint value)
        {
            return ToBytes((int)value);
        }
        public static byte[] ToBytes(this ulong value)
        {
            return ToBytes((long)value);
        }
        public unsafe static byte[] ToBytes(this float value)
        {
            return ToBytes(*(int*)(&value));
        }
        public unsafe static byte[] ToBytes(this double value)
        {
            return ToBytes(*(long*)(&value));
        }
        public static byte[] ToBytes(this string value)
        {
            int count = Encoding.Default.GetByteCount(value);
            byte[] ret = Take(count + sizeof(ushort));
            byte[] len = ToBytes((ushort)count);
            Array.Copy(len, 0, ret, 0,len.Length);
            Encoding.Default.GetBytes(value, 0, value.Length, ret, len.Length);
            Return(len);
            return ret;
        }
        public static byte[] Add(this byte[] head, byte[] tail, int startIndex, int length)
        {
            if (tail == null) return head;
            byte[] newB = Take(head.Length + length);
            Array.Copy(head, 0, newB, 0, head.Length);
            Array.Copy(tail, startIndex, newB, head.Length, length);
            Return(head);
            return newB;
        }
        public static byte[] Add(this byte[] head, byte[] tail)
        {
            if (tail == null) return head;
            return head.Add(tail, 0, tail.Length);
        }
        public static byte[] Add(this byte[] head, byte tail)
        {
            byte[] tai = Take(1);
            tai[0] = tail;
            return head.Add(tai, 0, tai.Length);
        }
    }
}
