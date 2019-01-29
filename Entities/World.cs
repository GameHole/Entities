 using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class World
    {
        internal FieldMgr fieldMgr;
        private Dictionary<uint, Entity> entities = new Dictionary<uint, Entity>();
        private Dictionary<Type, IShared> shareds = new Dictionary<Type, IShared>();
        private List<FieldsSystem> systems = new List<FieldsSystem>();
        public World()
        {
            fieldMgr = new FieldMgr();
            fieldMgr.Load();
        }
        public IT AddShared<IT, T>() where IT :IShared where T : class, IT, new()
        {
            T item = new T();
            (item as INeedWorld)?.Set(this);
            shareds.Add(typeof(IT), item);
            return item;
        }
        public void RemoveShared<IT>() where IT : IShared
        {
            (GetShared<IT>() as IDisposable)?.Dispose();
            shareds.Remove(typeof(IT));
        }
        public IT GetShared<IT>() where IT : IShared
        {
            return (IT)shareds[typeof(IT)];
        }
        public void Update(float tick)
        {
            foreach (var item in systems)
            {
                List<Entity> es = new List<Entity>();
                foreach (var e in entities.Values)
                {
                    if (HasFields(e, item.types))
                    {
                        es.Add(e);
                    }
                }
                item.entities = es;
                item.Update(tick);
            }
        }
        private bool HasFields(Entity entity,List<Type> types)
        {
            if (types == null || types.Count == 0) return false;
            foreach (var type in types)
            {
                if (!entity.Has(FieldIdMarker.allFileds[type]))
                {
                    return false;
                }
            }
            return true;
        }
        public void AddSystem<T>()where T:FieldsSystem,new()
        {
            systems.Add(new T());
        }
        private uint id;
        public Entity AddEntity()
        {
            Entity entity = new Entity(++id, this);
            entities.Add(entity.Id, entity);
            return entity;
        }
        public bool TryGetEntity(uint id,out Entity entity)
        {
            return entities.TryGetValue(id, out entity);
        }
        public bool HasEntity(uint id)
        {
            return entities.ContainsKey(id);
        }
        public void DestroyEntity(Entity entity)
        {
            if(entities.TryGetValue(entity.Id,out entity))
            {
                entity.Destroy();
                entities.Remove(entity.Id);
            }
        }
    }
}
