using System;
using System.Collections;
using System.Collections.Generic;

namespace Entities
{
   public struct NoGcStack<T>:IEnumerable<T>, IDisposable
    {
        //private static ArrayPool<T> pool;
        private T[] _items;
        private const int initCount = 32;
        private int _count;
        public int Count
        {
            get
            {
                if (_count <= 0) return 0;
                return _count;
            }
        }
        public NoGcStack(int mem)
        {
            _items = ArrayPool<T>.Pool.Take(mem);
            _count = 0;
        }
        private void _init()
        {
            if (_items == null)
                _items = ArrayPool<T>.Pool.Take(initCount);
        }
        public T Pop()
        {
            if (_count <= 0 || _items == null)
            {
                throw new StackOverflowException("Stack is empty can not take anything");
            }
            return _items[--_count];
        }
        public void Push(T item)
        {
            _init();
            _items[_count++] = item;
            if (_count >= _items.Length)
            {
                T[] ret = _items;
                _items = ArrayPool<T>.Pool.Take(_count * 2);
                Array.Copy(ret, 0, _items, 0, ret.Length);
                ArrayPool<T>.Pool.Return(ret);
            }
        }
        public T Peek()
        {
            if (this._count <= 0)
            {
                throw new StackOverflowException("Stack is empty can not take anything");
            }
            return _items[_count - 1];
        }
        public bool IsNull()
        {
            return _items == null;
        }
        public IEnumerator<T> GetEnumerator()
        {
            if (IsNull()) yield break;
            for (int i = 0; i < _count; i++)
            {
                yield return _items[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void Dispose()
        {
            if (IsNull()) return;
            ArrayPool<T>.Pool.Return(_items);
            this = new NoGcStack<T>();
        }

        public bool Contains(T item)
        {
            if (IsNull()) return false;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < this.Count; i++)
            {
                if (comparer.Equals(this._items[i], item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
