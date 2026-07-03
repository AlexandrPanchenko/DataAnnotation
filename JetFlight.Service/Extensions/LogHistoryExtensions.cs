using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using JetFlight.Shared.Models.LogHistory;

namespace JetFlight.Service.Extensions
{
    public static class LogHistoryExtensions
    {
        public class GenericEqualityComparer<T> : IEqualityComparer<T> where T : class
        {
            public bool Equals(T? x, T? y)
            {
                return IsEqualTo(x, y);
            }

            public int GetHashCode([DisallowNull] T obj)
            {
                return obj.GetHashCode();
            }
        }

        public static IEnumerable<LogHistoryItem<T, TKey>> FullOuterJoin<T, TKey>(this IEnumerable<T> left, IEnumerable<T> right, Func<T, TKey> keySelector) where TKey : notnull
        {
            var leftDict = left.ToDictionary(keySelector);
            var rightDict = right.ToDictionary(keySelector);
            var allIds = leftDict.Keys.Union(rightDict.Keys);

            return allIds.Select(key => new LogHistoryItem<T, TKey>
            {
                Key = key,
                Left = leftDict.GetValueOrDefault(key),
                Right = rightDict.GetValueOrDefault(key)
            }).ToList();
        }

        public static bool IsEqualTo<T>(this T? left, T? right) where T : class
        {
            var publicSimpleProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
            .ToArray();

            if (right == null && left == null) return true;
            if (right == null) return false;
            if (left == null) return false;

            foreach (var property in publicSimpleProperties)
            {
                var leftValue = property.GetValue(left);
                var rightValue = property.GetValue(right);

                if (!leftValue.EqualsNullable(rightValue))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool EqualsNullable<T>(this T right, T left)
        {
            if (EqualityComparer<T>.Default.Equals(right, default(T)) &&
                EqualityComparer<T>.Default.Equals(left, default(T)))
                return true;

            if (EqualityComparer<T>.Default.Equals(right, default(T))) return false;
            if (EqualityComparer<T>.Default.Equals(left, default(T))) return false;

            return EqualityComparer<T>.Default.Equals(right, left);
        }

        public static bool IsSimpleType(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsPrimitive ||
                   underlyingType == typeof(string) ||
                   underlyingType == typeof(DateTime) ||
                   underlyingType == typeof(DateTimeOffset) ||
                   underlyingType == typeof(TimeSpan) ||
                   underlyingType == typeof(Guid) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType.IsEnum;
        }
    }
}
