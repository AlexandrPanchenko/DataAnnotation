using System.Linq.Expressions;

namespace JetFlight.Shared.Models.Shared;

public static class LinqExtensions
{
    public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string columnName, OrderByDirectionTypes orderByDirectionType = OrderByDirectionTypes.ASC)
    {
        var parameter = Expression.Parameter(source.ElementType, "");
        var property = Expression.Property(parameter, columnName);
        var lambda = Expression.Lambda(property, parameter);
        var methodName = orderByDirectionType == OrderByDirectionTypes.ASC ? "OrderBy" : "OrderByDescending";
        var methodCallExpression = Expression.Call(typeof(Queryable),
            methodName,
            new Type[] { source.ElementType, property.Type },
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<TSource>(methodCallExpression);
    }
}