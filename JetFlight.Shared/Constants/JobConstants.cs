namespace JetFlight.Shared.Constants
{
    public static class JobConstants
    {
        public static int LoyaltyExpirationJobIntervalHours = 1;
        public static int LoyaltyExpirationJobDelayMinutes = 15;
        public const string LoyaltyGroup = nameof(LoyaltyGroup);
        public const string TargetNotificationGroup = nameof(TargetNotificationGroup);
        public const int TargetNotificationJobIntervalHours = 1;
        public const int TargetNotificationJobDelayMinutes = 30;
        public const string AnalyticsGroup = nameof(AnalyticsGroup);
        public const string ContentGroup = nameof(ContentGroup);
        public const int RfmSnapshotJobHour = 1;
        public const int RfmSnapshotJobMinute = 0;
        public const int PageAutoPublishJobIntervalMinutes = 30;
    }
}
