using System.Reflection;
using System.Runtime.Serialization;

namespace JetFlight.Shared.Extensions
{
    public static class EnumExtensions
    {
        public static string GetEnumMemberValue(this Enum enumValue)
        {
            var attribute = enumValue.GetCustomAttribute<EnumMemberAttribute>();
            return attribute?.Value;
        }

        public static T? GetCustomAttribute<T>(this Enum enumValue) where T : Attribute
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = field.GetCustomAttribute<T>();
            return attribute;
        }

        public static string? GetDisplayName(this Enum? enumValue)
        {
            var fieldInfo = enumValue?.GetType().GetField(enumValue.ToString());
            var attribute = fieldInfo?.GetCustomAttribute<EnumDisplayAttribute>();
            return attribute?.DisplayName ?? enumValue?.ToString();
        }
    }
}
