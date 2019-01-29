using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    sealed class FieldID<T>where T:struct,IField
    {
        public static int ID;
    }
}
