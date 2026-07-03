using JetFlight.Shared.Models.Shared;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.Service.Extensions
{
    public static class QueryResultExtensions
    {
        public static async Task<PagedListDTO<R>> GetPagedListAsync<T, R>(this IQueryable<T> query, PagingDTO pagingDTO, Func<T, R> map)
        {
            if (!string.IsNullOrWhiteSpace(pagingDTO.OrderBy))
            {
                query = query.OrderBy(pagingDTO.OrderBy, pagingDTO.OrderByDirectionType);
            }

            var result = new PagedListDTO<R>();
            result.TotalItems = await query.CountAsync();
            result.Offset = pagingDTO.Skip;
            result.Limit = pagingDTO.Take;

            var items = await query.Skip(pagingDTO.Skip)
                .Take(pagingDTO.Take)
                .ToListAsync();

            result.Items = items.Select(map).ToList();

            return result;
        }

        public static async Task<PagedListDTO<T>> GetPagedListAsync<T>(this IQueryable<T> query, PagingDTO pagingDTO)
        {
            if (!string.IsNullOrWhiteSpace(pagingDTO.OrderBy))
            {
                query = query.OrderBy(pagingDTO.OrderBy, pagingDTO.OrderByDirectionType);
            }

            var result = new PagedListDTO<T>();
            result.TotalItems = await query.CountAsync();
            result.Offset = pagingDTO.Skip;
            result.Limit = pagingDTO.Take;

            var items = await query.Skip(pagingDTO.Skip)
                .Take(pagingDTO.Take)
                .ToListAsync();

            result.Items = items;

            return result;
        }

        public static async Task<PagedListDTO<R>> GetPagedListAsync<T, R>(this IQueryable<T> query, PagingDTO pagingDTO, Func<T, Task<R>> asyncMap)
        {
            if (!string.IsNullOrWhiteSpace(pagingDTO.OrderBy))
            {
                query = query.OrderBy(pagingDTO.OrderBy, pagingDTO.OrderByDirectionType);
            }

            var result = new PagedListDTO<R>();
            result.TotalItems = await query.CountAsync();
            result.Offset = pagingDTO.Skip;
            result.Limit = pagingDTO.Take;

            var items = await query.Skip(pagingDTO.Skip)
                .Take(pagingDTO.Take)
                .ToListAsync();

            // Sequential mapping to avoid DbContext threading issues
            var mappedItems = new List<R>();
            foreach (var item in items)
            {
                mappedItems.Add(await asyncMap(item));
            }
            result.Items = mappedItems;

            return result;
        }
    }
}
