using System;

namespace ioi
{
    public static class ReflectionExtensions
    {
        public static bool IsInteger(this TypeCode typeCode)
            => typeCode >= TypeCode.SByte && typeCode <= TypeCode.UInt64;
    }
}