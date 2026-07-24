using System;
using System.Collections.Generic;

namespace ioi
{
    internal static class ClearAndDisposeExtensions
    {
        public static void ClearAndDispose<T>(this List<T> list)
        {
            foreach (var item in list)
            {
                if(item is IDisposable disposable) 
                    disposable.Dispose();
            }

            list.Clear();
        }
    }
}
