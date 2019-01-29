using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Entities
{
    public static class FieldIdMarker
    {
        public static int MaxTypeID = 0;
        public static Dictionary<Type,int> allFileds = new Dictionary<Type,int>();
        private static bool _isMarked;
        public static bool IsMarked() => _isMarked;
        public static void MarkAssemblies()
        {
            foreach (var Assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                MarkAssembly(Assembly);
            }
        }
        public static void MarkAssembly(Assembly assembly)
        {
            _isMarked = true;
            foreach (var item in assembly.GetTypes())
            {
                if (item.IsValueType && typeof(IField).IsAssignableFrom(item))
                {
                    var filed = typeof(FieldID<>).MakeGenericType(item)
                        .GetField("ID", BindingFlags.Static | BindingFlags.Public);
                    filed.SetValue(null, MaxTypeID);
                    allFileds.Add(item, MaxTypeID);
                    MaxTypeID++;
                }
            }
        }
    }
}
