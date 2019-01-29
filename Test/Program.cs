using System;
using System.Diagnostics;
using System.Threading;
using Entities;
using Entities.Net;
using System.Net;
namespace Test
{
    class Program
    {
        struct T0:IField
        {
            public int x;
        }
        struct T1 : IField
        {
            public int x;
        }
        struct T2 : IField
        {
            public int x;
        }
        struct T3 : IField
        {
            public int x;
        }
        sealed class TSystem : FieldsSystem
        {
            public TSystem()
            {
                AddFilter<T1>();
                AddFilter<T0>();
            }
            public override void Update(float tick)
            {
                foreach (var item in entities)
                {
                    item.Set(new T1() { x = item.Get<T0>().x });
                    // Console.WriteLine($"id::{item.Id},v::{item.Get<T1>().x},v1::{item.Get<T0>().x}");
                }
            }
        }
        [Flag(1)]
        struct TestPay : IToBytes
        {
            public int x;
            public void GetFrom(IByteStream stream)
            {
                x = stream.GetInt();
            }

            public byte[] ToBytes()
            {
                return x.ToBytes();
            }
        }
        sealed class TestDealer : PaylodHandler<TestPay>
        {
            protected override void Handle(Entity client, TestPay Msg)
            {
                Msg.x = 3;
                world.GetShared<INetShared>().Send(client, ref Msg);
                Console.WriteLine(Msg.x);
            }
        }
        static void Main(string[] args)
        {
            DebugUtility.Provide(new ConsoleLog());
            World world = new World();
            var net = world.AddShared<INetShared, WebSocketShared>();
            net.AsServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000));
            //for (int i = 0; i < 100; i++)
            //{
            //    var e = world.AddEntity();
            //    e.Add(new T1() { x = i });
            //    e.Add(new T2() { x = i + 1 });
            //    e.Add(new T0() { x = i+2 });
            //    e.Add(new T3() { x = i+3 });
            //}
            //world.AddSystem<TSystem>();
            //Stopwatch s = new Stopwatch();
            //while (true)
            //{
            //    s.Restart();
            //    world.Update(1);
            //    s.Stop();
            //    Console.WriteLine(s.Elapsed);
            //    Thread.Sleep(1000);
            //}
            Console.Read();
        }
    }
}
