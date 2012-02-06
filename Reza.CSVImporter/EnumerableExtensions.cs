using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reza.CSVImporter
{
    public static class EnumerableExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> obj, T value)
        {
            return obj
                .Select((a, i) => (a.Equals(value)) ? i : -1)
                .Max();
        }

        public static int IndexOf<T>(this IEnumerable<T> obj, T value
               , IEqualityComparer<T> comparer)
        {
            return obj
                .Select((a, i) => (comparer.Equals(a, value)) ? i : -1)
                .Max();
        }
    }
}
