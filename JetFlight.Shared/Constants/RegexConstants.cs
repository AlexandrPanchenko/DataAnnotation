namespace JetFlight.Shared.Constants
{
    public static class RegexConstants
    {
        public const string Latitude = "^(-?[0-8]?\\d(?:\\.\\d+)?|-?90(?:\\.0+)?)$";
        public const string Longitude = "^(-?(?:1[0-7]\\d|0?\\d{1,2})(?:\\.\\d+)?|-?180(?:\\.0+)?)$";
        public const string Email = "^[\\w+.-]+@([\\w-]+\\.)+[\\w-]{2,4}$";
        public const string PhysicalCard = "^7771[12]\\d{10}$";
    }
}
