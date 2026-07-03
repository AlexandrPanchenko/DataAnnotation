using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared
{
    public static class CollectionsHelper
    {
        public static bool AreCollectionsEqual<T>(List<T>? list1, List<T>? list2, Func<T, object> keySelector)
        {
            if (list1 == null && list2 == null)
            {
                return true;
            }

            if (list1 == null || list2 == null)
            {
                return false;
            }

            if (list1.Count != list2.Count)
            {
                return false;
            }

            return list1.OrderBy(keySelector).Select(keySelector)
                .SequenceEqual(list2.OrderBy(keySelector).Select(keySelector));
        }

        public static bool AreCollectionsEqual<T>(List<T>? list1, List<T>? list2, Func<T, object> keySelector, IEqualityComparer<T> comparer)
        {
            if (list1 == null && list2 == null)
            {
                return true;
            }

            if (list1 == null || list2 == null)
            {
                return false;
            }

            if (list1.Count != list2.Count)
            {
                return false;
            }

            return list1.OrderBy(keySelector).SequenceEqual(list2.OrderBy(keySelector), comparer);
        }
    }
}
