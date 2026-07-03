namespace JetFlight.ApplicationDataAccess.Helpers;

internal static class SeedRandomizer
{
    private static readonly Random Rng = new(42891);

    public static string Token(string prefix) => $"{prefix}-{Rng.Next(1000, 9999)}";

    public static string Label(string prefix) => $"{prefix} {Rng.Next(10, 99)}";

    public static IReadOnlyList<string> Labels(string prefix, int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => $"{prefix} {Rng.Next(100, 999)}-{i}")
            .ToList();
    }
}
