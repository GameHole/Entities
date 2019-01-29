using System;
using System.Collections;
using System.Collections.Generic;

namespace Entities.Net
{
    public struct PayList<T> : IToBytes, /*IEnumerable<T>, */IDisposable where T:IToBytes,new ()
    {
        private NoGcList<T> list;
        public PayList(int count) { list = new NoGcList<T>().MemSet(count); }
        public int Count => list.Count;
        public T this[int index] { get { return list[index]; } set { list[index] = value; } }
        public bool IsNull => list.IsNull();
        public void Dispose()
        {
            list.Dispose();
        }
        //public IEnumerator<T> GetEnumerator()
        //{
        //    return list.GetEnumerator();
        //}
        public void Add(T item)
        {
            //if (item == null) return;
            list.Add(item);
        }
        public void GetFrom(IByteStream stream)
        {
            if (!IsNull) Dispose();
            ushort count = stream.GetUShort();
            for (int i = 0; i < count; i++)
            {
                T item = new T();
                item.GetFrom(stream);
                list.Add(item);
            }
        }

        public byte[] ToBytes()
        {
            byte[] head = ((ushort)list.Count).ToBytes();
            for (int i = 0; i < list.Count; i++)
            {
                head = head.Add(list[i].ToBytes());
            }
            //foreach (var item in list)
            //{
               
            //}
            return head;
        }

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}
        public PayList<T> Clone()
        {
            PayList<T> ts = new PayList<T>(Count);
            for (int i = 0; i < list.Count; i++)
            {
                ts.list.Add(list[i]);
            }
            //foreach (var item in list)
            //{
            //    ts.list.Add(item);
            //}
            return ts;
        }
    }
}
