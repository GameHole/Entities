using System;
using Entities;
using Entities.Net;
using System.Net;
using System.Threading;
namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            World world = new World();
            var net = world.AddShared<INetShared, UdpShared>();
            net.AsServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000));

            while (true)
            {
                world.Update(0.166f);
                Thread.Sleep(16);
            }
        }
    }
}
