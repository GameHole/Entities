using System;
using System.Collections;
using System.Collections.Concurrent;
namespace Entities.Net
{
    public struct PayloadArray<T> :IEnumerable,IToBytes,IDisposable where T : struct, IToBytes
    {
        public void Dispose()
        { 
            array = null;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }
        public byte[] ToBytes()
        {
            if (array == null) return null;
            byte[] res = ((ushort)(array.Length)).ToBytes();
            for (int i = 0; i < array.Length; i++)
            {
                res = res.Add(array[i].ToBytes());
            }
            return res;
        }

        public void GetFrom(IByteStream stream)
        {
            if (!stream.HasMore()) return;
            ushort count = stream.GetUShort();
            array = new T[count];
            //if (count == 0) return;
            //if (array != null)
            //{
            //    Dispose();
            //}
            //array = Take(count);
            for (int i = 0; i < count; i++)
            {
                T item = new T();
                item.GetFrom(stream);
                array[i] = item;
            }
        }

        private T[] array;
        public PayloadArray(int count)
        {
            array = new T[count];
            //array = Take(count);
        }
        public PayloadArray<T> Clone()
        {
            PayloadArray<T> payloadArray = new PayloadArray<T>(array.Length);
            //payloadArray.array = Take(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                payloadArray.array[i] = array[i];
            }
            return payloadArray;
        }
        public int Length => array.Length;
        public bool isNull()
        {
            return array == null;
        }
        public T this[int index]
        {
            get
            {
                return array[index];
            }
            set
            {
                array[index] = value;
            }
        }
    }
}
