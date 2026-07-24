using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ioi.Extensions.EnumerableExtensions
{
    internal class UintRangeExtension
    {
        public static IEnumerable<uint> Range(uint start, uint count)
        {
            long max = ((long)start) + count - 1;
            return RangeIterator(start, count);
        }

        static IEnumerable<uint> RangeIterator(uint start, uint count)
        {
            for (uint i = 0; i < count; i++) yield return start + i;
        }
    }
}
