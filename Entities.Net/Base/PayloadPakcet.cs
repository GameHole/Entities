using System.Collections.Concurrent;
using System;
namespace Entities.Net
{
    class PayloadPakcet<T> : IPay where T : struct, IToBytes
    {
        private static readonly ConcurrentBag<PayloadPakcet<T>> pool = new ConcurrentBag<PayloadPakcet<T>>();

        public static PayloadPakcet<T> Take()
        {
            PayloadPakcet<T> item;
            if (!pool.TryTake(out item))
            {
                item = new PayloadPakcet<T>();
            }
            return item;
        }

        IPay IPay.Clone()
        {
            return Take();
        }

        internal static ushort Type;
        public T value;
        public void Dispose()
        {
            pool.Add(this);
        }

        public void GetFrom(IByteStream stream)
        {
            value.GetFrom(stream);
        }

        public byte[] ToBytes()
        {
            return value.ToBytes();
        }
    }
}
