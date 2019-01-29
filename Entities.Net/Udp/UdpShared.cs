using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Entities.Net
{
    public class UdpShared : INetShared,IDisposable
    {
        private Socket client;
        private readonly Dictionary<byte, ConDealer> conDealers = new Dictionary<byte, ConDealer>();
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        internal readonly MsgMaper msgMaper = new MsgMaper();
        internal World world;
        internal TaskCompletionSource<uint> connectTcs;
        internal TaskCompletionSource<int> disconnectTcs;
        

        public AddressFamily family = AddressFamily.InterNetwork;
        public int mtu = 1024;
        public uint Pid = ((1 << 15) | 0xE35B8A0) >> 3;

        internal readonly Dictionary<IPEndPoint, uint> entities = new Dictionary<IPEndPoint, uint>();

        public void Dispose()
        {
            cancellation.Cancel();
            client?.Dispose();
        }

        public void Set(World world)
        {
            this.world = world;
            msgMaper.Load(world);
        }
        public void AsServer(IPEndPoint endPoint)
        {
            conDealers.Add(ConDefine.connectReq, new Accept());
            conDealers.Add(ConDefine.msg, new DealMsg());
            conDealers.Add(ConDefine.disconnectReq, new ServerDisconnect());
            client = new Socket(family, SocketType.Dgram, ProtocolType.Udp);
            client.Bind(endPoint);
            Recv();
        }

        public Task<uint> Connect(IPEndPoint endPoint)
        {
            AsClient();
            return connect(endPoint);
        }

        public void Send<T>(Entity entity, ref T data) where T : struct, IToBytes
        {
            if (PayloadPakcet<T>.Type == 0)
            {
                throw new NotSupportMessageTypeException(typeof(T));
            }
            UdpField udpClient;
            if (entity.TryGet(out udpClient))
            {
                byte[] pay = Pid.ToBytes().Add(ConDefine.msg).Add(PayloadPakcet<T>.Type.ToBytes()).Add(data.ToBytes());
                try
                {
                    client.SendTo(pay, udpClient.endPoint);
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
        }
        private void AsClient()
        {
            conDealers.Add(ConDefine.onConnect, new onConnect());
            conDealers.Add(ConDefine.msg, new DealMsg());
            conDealers.Add(ConDefine.onDisconect, new onDisconnect());
            client = new Socket(family, SocketType.Dgram, ProtocolType.Udp);
            client.Bind(new IPEndPoint(IPAddress.Any, 0));
            Recv();
        }
        private async void Recv()
        {
            IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
            while (!cancellation.Token.IsCancellationRequested)
            {
                try
                {
                    UdpResult result = await ReceveFromAsync(point).ConfigureAwait(false);
                    if (Pid != result.byteStream.GetUInt()) continue;
                    ConDealer dealer;
                    if (conDealers.TryGetValue(result.byteStream.GetByte(), out dealer))
                    {
                        dealer.Run(result, this);
                    }
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10054) continue;
                }
                catch (Exception e)
                {
                    DebugUtility.Error(e);
                }
            }
        }
        private Task<UdpResult> ReceveFromAsync(EndPoint end)
        {
            return Task.Factory.StartNew(() =>
            {
                byte[] buffer = unSafeByteHelper.Take(mtu);
                int n = client.ReceiveFrom(buffer, ref end);
                ByteStream stream = ByteStream.Take();
                stream.Set(buffer, 0, n);
                UdpResult result = new UdpResult() { endPoint = (IPEndPoint)end, byteStream = stream };
                unSafeByteHelper.Return(buffer);
                return result;
            }, cancellation.Token);
        }


        private Task<uint> connect(IPEndPoint point)
        {
            var sock = world.AddEntity();
            sock.Add(new UdpField(point));
            entities.Add(point, sock.Id);
            connectTcs = new TaskCompletionSource<uint>();
            SendHead(point, ConDefine.connectReq);
            return connectTcs.Task;
        }
        internal void SendHead(IPEndPoint endPoint, byte head)
        {
            byte[] send = Pid.ToBytes().Add(head);
            client.SendTo(send, endPoint);
            unSafeByteHelper.Return(send);
        }
        public async Task<int> Disconnect(uint socket)
        {
            Entity entity;
            if (world.TryGetEntity(socket, out entity))
            {
                UdpField udpClient;
                if (entity.TryGet(out udpClient))
                {
                    disconnectTcs = new TaskCompletionSource<int>();
                    SendHead(udpClient.endPoint, ConDefine.disconnectReq);
                    return await disconnectTcs.Task;
                }
            }
            return -1;
        }
    }
}
