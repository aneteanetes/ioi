using System.Collections.Generic;
using System.Linq;

namespace ioi
{
    internal static class OneOfExtensions
    {
        public static bool OneOf<T>(this T @object, IEnumerable<T> @enum)
        {
            return @enum.Contains(@object); 
        }
    }
}
