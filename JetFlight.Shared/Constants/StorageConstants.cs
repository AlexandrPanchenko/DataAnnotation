namespace JetFlight.Shared.Constants
{
    public static class StorageConstants
    {
        public static string PhysicalPath => Environment.GetEnvironmentVariable("STORAGE_PATH")!;
        public const string AppPath = "/app/storage";
    }
}
