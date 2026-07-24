using System;
using System.Collections.Generic;
using System.Linq;

namespace ioi
{
    internal static class GetAllEnumValuesExtensions
    {
        public static IEnumerable<T> GetAllValues<T>(this Type enumType)
        {
            return Enum.GetValues(enumType).Cast<T>();
        }
    }
}
