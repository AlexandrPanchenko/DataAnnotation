namespace JetFlight.Shared.Helpers;

/// <summary>
/// Сортування магазинів: спочатку вулиці (вул./вулиця), потім площі (пл.), потім проспекти (пр-т/проспект/пр.), решта — в кінець.
/// </summary>
public static class StoreAddressSortHelper
{
    /// <summary>
    /// Порядок типу адреси: 1 = вулиці, 2 = площі, 3 = проспекти, 4 = інше.
    /// </summary>
    public static int GetStreetTypeOrder(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return 4;

        var value = address.TrimStart().ToLowerInvariant();

        if (value.StartsWith("вул.") || value.StartsWith("вул ") || value.StartsWith("вулиця "))
            return 1; // вулиці

        if (value.StartsWith("пл.") || value.StartsWith("площа"))
            return 2; // площі

        // проспекти: перевіряємо довші варіанти спочатку
        if (value.StartsWith("проспект") || value.StartsWith("просп.") || value.StartsWith("пр-т") || value.StartsWith("пр."))
            return 3; // проспекти

        return 4; // інше
    }

    /// <summary>
    /// Повертає частину адреси після типу (назву вулиці/площі/проспекту) для алфавітного сортування.
    /// </summary>
    public static string GetStreetNamePart(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return string.Empty;

        var trimmed = address.Trim();
        var parts = trimmed.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        return parts.Length >= 2 ? parts[1] : trimmed;
    }
}
