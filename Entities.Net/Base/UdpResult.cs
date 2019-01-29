using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace Entities.Net
{
    public struct UdpResult
    {
        public IByteStream byteStream;
        public IPEndPoint endPoint;
    }
}
