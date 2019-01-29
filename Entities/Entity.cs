using System;

namespace Entities
{
    public struct Entity : IEquatable<Entity>
    {
        public const uint NULL_VALUE = uint.MaxValue;
        public readonly uint Id;
        private World world;
        private uint[] indexs;
        internal Entity(uint id,World world)
        {
            Id = id;
            this.world = world;
            indexs = new uint[FieldIdMarker.MaxTypeID];
            for (int i = 0; i < indexs.Length; i++)
            {
                indexs[i] = NULL_VALUE;
            }
        }
        public void Add<T>(T value)where T : struct, IField
        {
            indexs[FieldID<T>.ID] = world.fieldMgr.GetArray<T>().Add(ref value);
        }
        public void Add<T>() where T : struct, IField
        {
            Add<T>(new T());
        }
        internal bool Has(int id)
        {
            return indexs[id] != NULL_VALUE;
        }
        public bool Has<T>() where T : struct, IField
        {
            return indexs[FieldID<T>.ID] != NULL_VALUE;
        }
        public T Get<T>() where T : struct, IField
        {
            return world.fieldMgr.GetArray<T>().array[indexs[FieldID<T>.ID]];
        }
        public void Set<T>(T value) where T : struct, IField
        {
            world.fieldMgr.GetArray<T>().array[indexs[FieldID<T>.ID]] = value;
        }
        public void Set<T>(ref T value) where T : struct, IField
        {
            world.fieldMgr.GetArray<T>().array[indexs[FieldID<T>.ID]] = value;
        }
        public bool TryGet<T>(out T value) where T : struct, IField
        {
            int index = FieldID<T>.ID;
            if (Has(index))
            {
                value = world.fieldMgr.GetArray<T>().array[indexs[index]];
                return true;
            }
            value = default(T);
            return false;
        }
        public void Remove<T>() where T : struct, IField
        {
            int typeId = FieldID<T>.ID;
            if (!Has(typeId)) return;
            world.fieldMgr.GetArray<T>().Remove(indexs[typeId]);
            indexs[FieldID<T>.ID] = NULL_VALUE;
        }
        internal void Destroy()
        {
            for (int i = 0; i < indexs.Length; i++)
            {
                if (indexs[i] == NULL_VALUE) continue;
                
                world.fieldMgr.GetArray(i).Remove(indexs[i]);
            }
        }
        public override bool Equals(object obj)
        {
            return obj is Entity && Equals((Entity)obj);
        }

        public bool Equals(Entity other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(Entity a, Entity b) => a.Id == b.Id;
        public static bool operator !=(Entity a, Entity b) => a.Id != b.Id;
    }
}
