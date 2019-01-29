using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Entities.Net
{
    public class WebSocketShared : INetShared
    {
        private WebSocket webSocket;
        private readonly MsgMaper maper = new MsgMaper();
        private Dictionary<WebSocket, Entity> clients = new Dictionary<WebSocket, Entity>();

        public void AsServer(IPEndPoint endPoint)
        {
            webSocket = new WebSocket(endPoint);
            webSocket.Listen();
            accept();
        }

        public async Task<uint> Connect(IPEndPoint endPoint)
        {
            webSocket = new WebSocket();
            Entity entity = world.AddEntity();
            entity.Add(new NetworkData() { webSocket = webSocket });
            await webSocket.Connect(endPoint);
            return entity.Id;
        }

        public void Send<T>(Entity entity, ref T data) where T : struct, IToBytes
        {
            var socket = entity.Get<NetworkData>().webSocket;
            if (PayloadPakcet<T>.Type == 0)
            {
                throw new NotSupportMessageTypeException(typeof(T));
            }
            byte[] pay = PayloadPakcet<T>.Type.ToBytes().Add(data.ToBytes());
            try
            {
                socket.SendAsync(pay, 0, pay.Length);
            }
            catch (Exception e)
            {
                DebugUtility.Error(e);
                throw;
            }
            finally
            {
                unSafeByteHelper.Return(pay);
            }
        }
        private World world;
        public void Set(World world)
        {
            this.world = world;
            maper.Load(world);
        }

        private async void accept()
        {
            while (true)
            {
                WebSocket ws = null;
                try
                {
                    ws = await webSocket.AcceptAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (ws != null) ws.Dispose();
                    Console.WriteLine($"Error:{e}");
                    continue;
                }
                Entity entity = world.AddEntity();
                ws.onClose += (s) =>
                {
                    Entity removed;
                    if (clients.TryGetValue(ws, out removed))
                    {
                        world.DestroyEntity(removed);
                        clients.Remove(ws);
                    }
                };
                entity.Add(new NetworkData() { webSocket = ws });
                receve(entity);
            }
        }
        private async void receve(Entity entity)
        {
            var websocket = entity.Get<NetworkData>().webSocket;
            while (true)
            {
                if (!world.HasEntity(entity.Id)) return;
                ByteStream stream = null;
                try
                {
                    byte[] rec = await websocket.ReceiveAsync().ConfigureAwait(false);
                    stream = ByteStream.Take();
                    stream.Set(rec);
                    ushort func = stream.GetUShort();
                    using (IPay paylodable = maper.TakeMsgClone(func))
                    {
                        if (paylodable == null) return;
                        paylodable.GetFrom(stream);
                        ADealer dealer = maper.TakeDealer(func);
                        dealer?.Run(entity, paylodable);
                    }
                }
                catch (Exception e)
                {
                    DebugUtility.Error($"Error::{e}");
                }
                finally
                {
                    if (stream != null)
                    {
                        ByteStream.Return(stream);
                    }
                }
            }
        }
    }
}
