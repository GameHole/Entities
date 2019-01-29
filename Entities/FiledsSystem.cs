using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public abstract class FieldsSystem
    {
        public World world;
        internal List<Type> types = new List<Type>();
        public List<Entity> entities;
        public void AddFilter<T>()where T:struct,IField
        {
            types.Add(typeof(T));
        }
        public abstract void Update(float tick);
    }
}
