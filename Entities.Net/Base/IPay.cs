using System;
namespace Entities.Net
{
    public interface IPay : IToBytes, IDisposable
    {
        IPay Clone();
    }
}
