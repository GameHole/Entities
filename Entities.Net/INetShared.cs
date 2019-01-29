using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace Entities.Net
{
    public interface INetShared:IShared,INeedWorld
    {
        void AsServer(IPEndPoint endPoint);
        Task<uint> Connect(IPEndPoint endPoint);
        void Send<T>(Entity entity, ref T data) where T : struct, IToBytes;
    }
}
