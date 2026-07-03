namespace JetFlight.Shared.Constants
{
    public static class TimeZoneConstants
    {
        public const string UATimezone = "Europe/Kyiv";
        public const string UATimezoneWindows = "FLE Standard Time";

        public const string EEST = "Eastern European Standard Time";

        public static TimeZoneInfo ResolveUATimeZone()
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(UATimezone, out var tz))
            {
                return tz;
            }

            return TimeZoneInfo.FindSystemTimeZoneById(UATimezoneWindows);
        }
    }
}
