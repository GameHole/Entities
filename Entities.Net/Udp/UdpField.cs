using System.Net;
namespace Entities.Net
{
    public struct UdpField : IField
    {
        public readonly IPEndPoint endPoint;
        public UdpField(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }
    }
}
