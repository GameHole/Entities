using System;
using System.Collections.Generic;

namespace Entities
{
    class FieldMgr
    {
        private List<IFieldArray> array = new List<IFieldArray>();
        public void Load()
        {
            if (!FieldIdMarker.IsMarked())
            {
                FieldIdMarker.MarkAssemblies();
            }
            foreach (var item in FieldIdMarker.allFileds.Keys)
            {
                Type arrayType = typeof(FieldArray<>).MakeGenericType(item);
                array.Add(Activator.CreateInstance(arrayType) as IFieldArray);
            }
        }
        public FieldArray<T> GetArray<T>()where T : struct, IField
        {
            return array[FieldID<T>.ID] as FieldArray<T>;
        }
        public IFieldArray GetArray(int index)
        {
            return array[index];
        }
    }
    interface IFieldArray
    {
        void Remove(uint index);
    }
    class FieldArray<T>: IFieldArray where T:struct,IField
    {
        internal T[] array;
        private uint _count;
        private Stack<uint> removed = new Stack<uint>();
        public uint Add(ref T item)
        {
            uint index;
            if (removed.Count > 0)
            {
                index = removed.Pop();
            }
            else
            {
                if (array == null)
                {
                    array = new T[4];
                }
                if (_count >= array.Length)
                {
                    var newArray = new T[array.Length * 2];
                    Array.Copy(array, 0, newArray, 0, array.Length);
                    array = newArray;
                }
                index = _count++;
            }
            array[index] = item;
            return index;
        }
        public void Remove(uint index)
        {
            removed.Push(index);
        }
    }
}
