using NodaTime;

namespace JetFlight.Service.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime FromUtcToTimezone(this DateTime dateTime, string timeZoneId)
        {
            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            var instant = Instant.FromDateTimeUtc(dateTime);
            var zonedDateTime = instant.InZone(timeZone);
            return zonedDateTime.ToDateTimeUnspecified();
        }

        public static DateTimeOffset FromUtcToTimezoneOffset(this DateTime dateTime, string timeZoneId)
        {
            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            var instant = Instant.FromDateTimeUtc(dateTime);
            var zonedDateTime = instant.InZone(timeZone);
            return zonedDateTime.ToDateTimeOffset();
        }

        public static DateTime FromTimeZoneToUtc(this DateTime dateTime, string timeZoneId)
        {
            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            var zonedDateTime = LocalDateTime.FromDateTime(dateTime)
                         .InZoneStrictly(timeZone);

            return zonedDateTime.ToDateTimeUtc();
        }
    }
}
