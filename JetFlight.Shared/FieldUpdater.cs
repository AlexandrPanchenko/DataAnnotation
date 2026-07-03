
namespace JetFlight.Shared
{
    public static class FieldUpdater
    {
        public static T UpdateIfNotNullOrEmpty<T>(T target, T source)
        {
            if (source is string str && !string.IsNullOrEmpty(str))
            {
                return source;
            }
            else if (!EqualityComparer<T>.Default.Equals(source, default))
            {
                return source;
            }
            return target;
        }
    }
}
