using System;
using System.Threading.Tasks;

namespace Entities.Net
{
    static class ConDefine
    {
        public const byte connectReq = 0;
        public const byte onConnect = 1;
        public const byte msg = 2;
        public const byte disconnectReq = 3;
        public const byte onDisconect = 4;
    }
    public abstract class ConDealer
    {
        public async void Run(UdpResult result, UdpShared world)
        {
            await RunAsync(result, world).ConfigureAwait(false);
            ByteStream.Return(result.byteStream as ByteStream);
        }
        protected abstract Task RunAsync(UdpResult result, UdpShared udpshared);
    }
    class Accept : ConDealer
    {
        protected override Task RunAsync(UdpResult result, UdpShared udpshared)
        {
            return Task.Factory.StartNew(() =>
            {

                if (!udpshared.entities.ContainsKey(result.endPoint))
                {
                    Entity e = udpshared.world.AddEntity();
                    e.Add(new UdpField(result.endPoint));
                    udpshared.entities.Add(result.endPoint, e.Id);
                    //DebugUtility.Log($"Accept::RunAsync ID={e.Id}");
                    udpshared.SendHead(result.endPoint, ConDefine.onConnect);
                }
            });
        }
    }
    class DealMsg : ConDealer
    {
        protected override Task RunAsync(UdpResult result, UdpShared udpshared)
        {
            return Task.Factory.StartNew(() =>
            {
                uint eid;
                if (udpshared.entities.TryGetValue(result.endPoint, out eid))
                {
                    ushort func = result.byteStream.GetUShort();
                    //DebugUtility.Log($"DealMsg::func::{func}");
                    using (IPay paylodable = udpshared.msgMaper.TakeMsgClone(func))
                    {
                        if (paylodable == null) return;
                        //DebugUtility.Log($"DealMsg::func::{func}");
                        paylodable.GetFrom(result.byteStream);
                        ADealer dealer = udpshared.msgMaper.TakeDealer(func);
                        Entity entity;
                        if (udpshared.world.TryGetEntity(eid, out entity))
                        {
                            dealer?.Run(entity, paylodable);
                        }
                    }
                    //DebugUtility.Log($"DealMsg:: id::{entity.Id}");
                }
            });
        }
    }
    class ServerDisconnect : ConDealer
    {
        protected override Task RunAsync(UdpResult result, UdpShared udpshared)
        {
            return Task.Factory.StartNew(() =>
            {
                uint eid;
                if (udpshared.entities.TryGetValue(result.endPoint, out eid))
                {
                    Entity e;
                    if (udpshared.world.TryGetEntity(eid, out e))
                    {
                        udpshared.world.DestroyEntity(e);
                        udpshared.entities.Remove(result.endPoint);
                    }
                    udpshared.SendHead(result.endPoint, ConDefine.onDisconect);
                }
            });
        }
    }
    class onConnect : ConDealer
    {
        protected override Task RunAsync(UdpResult result, UdpShared udpshared)
        {
            uint eid;
            udpshared.entities.TryGetValue(result.endPoint, out eid);
            udpshared.connectTcs.SetResult(eid);
            return Task.CompletedTask;
        }
    }
    class onDisconnect : ConDealer
    {
        protected override Task RunAsync(UdpResult result, UdpShared udpshared)
        {
            uint eid;
            udpshared.entities.TryGetValue(result.endPoint, out eid);
            udpshared.disconnectTcs.SetResult(0);
            return Task.CompletedTask;
        }
    }
}
