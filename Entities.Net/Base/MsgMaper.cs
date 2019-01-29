using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Net
{

    [Serializable]
    public class NotSupportMessageTypeException : Exception
    {
        public NotSupportMessageTypeException() { }
        public NotSupportMessageTypeException(Type type) : base($"Type = {type},Flag = 0" +
            $"\n-->If it is a real message,you can use [Flag(ushort type)] to supporting it!!") { }
        public NotSupportMessageTypeException(string message) : base(message) { }
        public NotSupportMessageTypeException(string message, Exception inner) : base(message, inner) { }
        protected NotSupportMessageTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    public class MsgMaper
    {
        private readonly Dictionary<ushort, IPay> typeMap = new Dictionary<ushort, IPay>();
        private readonly Dictionary<ushort, ADealer> bhrmaper = new Dictionary<ushort, ADealer>();
        public IPay TakeMsgClone(ushort funcs)
        {
            IPay paylodable;
            if (!typeMap.TryGetValue(funcs, out paylodable))
            {
                return null;
            }
            return paylodable.Clone();
        }
        public ADealer TakeDealer(ushort type)
        {
            ADealer m;
            bhrmaper.TryGetValue(type, out m);
            return m;
        }
        public void Load(World world)
        {
            foreach (var ab in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var item in ab.GetTypes())
                {
                    if (item.IsAbstract) continue;
                    if (typeof(ADealer).IsAssignableFrom(item) && item.BaseType.IsGenericType)
                    {
                        var dealer = (Activator.CreateInstance(item) as ADealer);
                        dealer.Setworld(world);
                        Type msgType = item.BaseType.GetGenericArguments()[0];
                        var flag = msgType.GetCustomAttribute<FlagAttribute>();
                        if (flag == null)
                        {
                            throw new NotSupportMessageTypeException(msgType);
                        }
                        bhrmaper.Add(flag.Type, dealer);
                        //DebugUtility.Log("dealer::" + item);
                        //DebugUtility.Log("msg::" + msgType);
                    }
                }
                foreach (var item in ab.GetTypes())
                {
                    var flag = item.GetCustomAttribute<FlagAttribute>();
                    if (flag != null && typeof(IToBytes).IsAssignableFrom(item))
                    {
                        Type paylod = typeof(PayloadPakcet<>);
                        paylod = paylod.MakeGenericType(item);
                        FieldInfo info = paylod.GetField("Type", BindingFlags.Static | BindingFlags.NonPublic);
                        info.SetValue(null, flag.Type);
                        if (!bhrmaper.ContainsKey(flag.Type)) continue;
                        var cloneable = Activator.CreateInstance(paylod) as IPay;
                        //DebugUtility.Log(cloneable);
                        typeMap.Add(flag.Type, cloneable);
                        //DebugUtility.Log("msg::" + item + ",msgType::" + flag.Type);
                    }
                }
            }
           
            
        }
    }
}
