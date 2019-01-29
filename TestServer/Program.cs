using Entities;
using Entities.Net;
using System;
using System.Net;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TT();
            Console.Read();
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
                //Msg.x = 3;
                //world.GetShared<INetShared>().Send(client, ref Msg);
                Console.WriteLine(Msg.x);
            }
        }
        static async void TT()
        {
            World world = new World();
            var net = world.AddShared<INetShared, WebSocketShared>();
            uint id = await net.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000));
            Console.WriteLine(id);
            Entity e;
            if (world.TryGetEntity(id, out e))
            {
                TestPay test = new TestPay() { x = 500 };
                net.Send(e, ref test);
            }
        }
    }
}
