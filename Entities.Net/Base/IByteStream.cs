namespace Entities.Net
{
    public interface IByteStream
    {
        void Set(byte[] buffer, int index, int lenght);
        //void Set(byte[] buffer );
        bool HasMore();
        bool Enough(int count);
        void MoveIndex(int length);
        int GetInt(bool peek=false);
        uint GetUInt(bool peek = false);
        byte GetByte(bool peek = false);
        short GetShort(bool peek = false);
        ushort GetUShort(bool peek = false);
        long GetLong(bool peek = false);
        ulong GetULong(bool peek = false);
        float GetFloat(bool peek = false);
        double GetDouble(bool peek = false);
        bool GetBool(bool peek = false);
        string GetString(bool peek = false);
    }
}
