using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    using Entities.Net;
    [Flag(1)]
    struct Join : IToBytes
    {
        public void GetFrom(IByteStream stream)
        {
        }

        public byte[] ToBytes()
        {
            return null;
        }
    }
    [Flag(1)]
    struct JoinOk : IToBytes
    {
        public void GetFrom(IByteStream stream)
        {
        }

        public byte[] ToBytes()
        {
            return null;
        }
    }
}
