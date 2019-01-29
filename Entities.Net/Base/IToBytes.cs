namespace Entities.Net
{
    public interface IToBytes
    {
        byte[] ToBytes();
        void GetFrom(IByteStream stream);
    }
}
