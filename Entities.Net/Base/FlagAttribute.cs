using System;
namespace Entities.Net
{
    public class FlagAttribute : Attribute
    {
        public ushort Type;
        public FlagAttribute(ushort type)
        {
            Type = type;
        }
    }
}
