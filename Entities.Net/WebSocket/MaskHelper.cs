using System;
using System.Security.Cryptography;
namespace Entities.Net
{
    class OpCode
    {
        public const byte Cont = 0x0;
        public const byte Text = 0x1;
        public const byte Binary = 0x2;
        public const byte Close = 0x8;
        public const byte Ping = 0x9;
        public const byte Pong = 0xa;
    }
    static class MaskHelper
    {
        static readonly RandomNumberGenerator generator = new RNGCryptoServiceProvider();
        public static void UnMask(byte[] data, byte[] mask)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ mask[i % 4]);
            }
        }
        public static byte[] Mask(byte[] data,int offset,int size,byte opCode)
        {
            int len = 2;
            bool exten = size >= 126;
            if (exten) len += 2;
            byte[] head = new byte[len];
            head[0] = (byte)(0x80 | opCode);
            if (exten)
            {
                head[1] = /*0x80 |*/ 126;
                GetBytes((ushort)size, head, 2);
            }
            else
            {
                head[1] = (byte)(/*0x80 |*/ size);
            }
            //byte[] key = new byte[4];
            //generator.GetBytes(key);
            //Array.Copy(key, 0, head, len - 4, 4);
            byte[] result = new byte[size + len];
            Array.Copy(head, 0, result, 0, head.Length);
            Array.Copy(data, offset, result, head.Length, size);
            //for (int i = offset; i < size; i++)
            //{
            //    result[i + len] = (byte)(data[i] ^ key[i % 4]);
            //}
            return result;
        }
        static unsafe void GetBytes(ushort value,byte[] res,int offset)
        {
            byte* c = (byte*)(&value);
            res[offset] = *c;
            res[offset + 1] = *(c + 1);
        }
    }
}
