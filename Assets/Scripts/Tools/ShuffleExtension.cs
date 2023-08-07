using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeCrawl.Utils
{
    public static class ShuffleExtension
    {
        public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> enumerable)
        {
            return enumerable.OrderBy(_ => System.Guid.NewGuid());
        }
    }
}
