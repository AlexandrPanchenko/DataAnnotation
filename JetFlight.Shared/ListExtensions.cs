using JetFlight.Shared.Models.Shared;

namespace JetFlight.Shared;

public static class ListExtensions
{
    public static List<T> GetPaginatedList<T>(this List<T> input, PagingDTO pagingDTO)
    {
        if (!string.IsNullOrWhiteSpace(pagingDTO.OrderBy))
        {
            input = input.AsQueryable().OrderBy(pagingDTO.OrderBy, pagingDTO.OrderByDirectionType).ToList();
        }

        return input.Skip(pagingDTO.Skip)
            .Take(pagingDTO.Take)
            .ToList();
    }

    public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
    {
        foreach (var value in list)
        {
            await func(value);
        }
    }
}