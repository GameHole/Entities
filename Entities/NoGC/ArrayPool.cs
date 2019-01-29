using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Entities
{
    public class ArrayPool<T>
    {
        public static readonly ArrayPool<T> Pool = new ArrayPool<T>();
        private readonly ConcurrentDictionary<int, Stack<T[]>> pools = new ConcurrentDictionary<int, Stack<T[]>>();
        public T[] Take(int count)
        {
            Stack<T[]> link = pools.GetOrAdd(count, (key) =>
             {
                 return new Stack<T[]>();
             });
            if (link.Count > 0)
            {
                //T[] item = link.First.Value;
                //link.RemoveFirst();
                return link.Pop();
            }
            else
            {
                return new T[count];
            }
        }
        public void Return(T[] item)
        {
            Stack<T[]> link = pools.GetOrAdd(item.Length, (key) =>
            {
                return new Stack<T[]>();
            });
            link.Push(item);
        }
    }
}