using System;
using System.Collections;
using System.Collections.Generic;
namespace Entities
{
    public struct NoGcList<T> :/* IEnumerable<T>, IList<T>,*/ IDisposable
    {
        //private static ArrayPool<T> pool;
        private static readonly T[] _emptyArray;
        static NoGcList()
        {
            //pool = new ArrayPool<T>();
            _emptyArray = new T[0];
        }
        private T[] _items;
        //private int _version;
        public T this[int index] { get { return _items[index]; } set { _items[index] = value;/* this._version++;*/ } }

        public int Count { get; private set; }

        public bool IsReadOnly => false;
        public bool IsNull()
        {
            return _items == null;
        }
        public NoGcList(int capacity)
        {
            Count = 0;
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (capacity == 0)
            {
                this._items = _emptyArray;
            }
            else
            {
                this._items = ArrayPool<T>.Pool.Take(capacity);
            }
        }
        public void Sort(IComparer<T> comparison)
        {
            if (comparison == null)
            {
                throw new ArgumentNullException();
            }
            if (this.Count > 0)
            {
                //IComparer<T> comparer = new Array(comparison);
                Array.Sort<T>(this._items, 0, this.Count, comparison);
            }
        }
        public NoGcList<T> MemSet(int count)
        {
            this = new NoGcList<T>(count);
            this.Count = count;
            return this;
        }
        public void Add(T item)
        {
            if (IsNull()) _items = _emptyArray;
            if (this.Count == this._items.Length)
            {
                this.EnsureCapacity(this.Count + 1);
            }
            int index = this.Count;
            this.Count = index + 1;
            this._items[index] = item;
           // this._version++;
        }
        private void EnsureCapacity(int min)
        {
            if (this._items.Length < min)
            {
                int num = (this._items.Length == 0) ? 4 : (this._items.Length * 2);
                if (num > 0x7fefffff)
                {
                    num = 0x7fefffff;
                }
                if (num < min)
                {
                    num = min;
                }
                this.Capacity = num;
            }
        }
        public int Capacity
        {
            get
            {
                return this._items.Length;
            }
            set
            {
                if (value < this.Count)
                {
                    throw new ArgumentOutOfRangeException("must bigger then size");
                }
                if (value != this._items.Length)
                {
                    if (value > 0)
                    {
                        T[] destinationArray = ArrayPool<T>.Pool.Take(value);
                        if (this.Count > 0)
                        {
                            Array.Copy(this._items, 0, destinationArray, 0, this.Count);
                        }
                        if (_items != _emptyArray)
                            ArrayPool<T>.Pool.Return(_items);
                        this._items = destinationArray;
                    }
                    else
                    {
                        this._items = _emptyArray;
                    }
                }
            }
        }
        public void Clear()
        {
            if (this.Count > 0)
            {
                Array.Clear(this._items, 0, this.Count);
                this.Count = 0;
            }
            //this._version++;
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
        public void CopyTo(T[] array)
        {
            this.CopyTo(array, 0);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this._items, 0, array, arrayIndex, this.Count);
        }
        public int IndexOf(T item) => Array.IndexOf<T>(this._items, item, 0, this.Count);
        public int IndexOf(T item, int index, int count)
        {
            return Array.IndexOf(this._items, item, index, count);
        }

        public void Insert(int index, T item)
        {
            if (index > this.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (this.Count == this._items.Length)
            {
                this.EnsureCapacity(this.Count + 1);
            }
            if (index < this.Count)
            {
                Array.Copy(this._items, index, this._items, index + 1, this.Count - index);
            }
            this._items[index] = item;
            this.Count++;
            //this._version++;
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if (index >= this.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.Count--;
            if (index < this.Count)
            {
                Array.Copy(this._items, index + 1, this._items, index, this.Count - index);
            }
            this._items[this.Count] = default(T);
           // this._version++;
        }

       /* IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<T> GetEnumerator()
        {
            if (IsNull()) yield break;
            for (int i = 0; i < Count; i++)
            {
                yield return _items[i];
            }
        }*/

        public void Dispose()
        {
            if (IsNull()) return;
            ArrayPool<T>.Pool.Return(_items);
            this = new NoGcList<T>();
        }
    }
}
