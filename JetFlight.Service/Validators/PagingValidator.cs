using JetFlight.Shared.Models.Shared;

namespace JetFlight.Service.Validators;

public static class PagingValidator
{
    public static ValidatePagingParamsDTO ValidatePagingParams(Type type,
        string? orderBy,
        string? orderByDirection,
        int? offset,
        int? limit,
        int maxLimit,
        IList<string>? unsupportedOrderByValues = null)
    {
        orderBy = orderBy ?? string.Empty;
        orderByDirection = orderByDirection ?? OrderByDirectionTypes.ASC.ToString();

        var response = new ValidatePagingParamsDTO
        {
            PagingDTO =
            {
                Skip = offset ?? 0,
                Take = (limit.HasValue && limit.Value > 0) ? limit.Value : maxLimit
            }
        };

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            if (unsupportedOrderByValues != null
                && unsupportedOrderByValues.Any(x => x.Equals(orderBy, StringComparison.OrdinalIgnoreCase)
                                                     || unsupportedOrderByValues.Any(x => orderBy.StartsWith($"{x}.", StringComparison.OrdinalIgnoreCase))))
            {
                response.Errors.Add("OrderBy parameter value is not supported");
            }
            else
            {
                response.PagingDTO.OrderBy = orderBy;
            }
        }
        else
        {
            response.PagingDTO.OrderBy = orderBy;
        }

        if (Enum.TryParse(orderByDirection, true, out OrderByDirectionTypes orderByDirectionType))
        {
            response.PagingDTO.OrderByDirectionType = orderByDirectionType;
        }
        else
        {
            response.Errors.Add("Invalid orderBy direction parameter");
        }

        if (response.PagingDTO.Skip < 0)
        {
            response.Errors.Add("Invalid skip parameter");
        }

        if (response.PagingDTO.Take < 0)
        {
            response.Errors.Add("Invalid limit parameter");
        }
        else if (response.PagingDTO.Take > maxLimit)
        {
            response.Errors.Add("Max limit parameter exceeded");
        }

        return response;
    }
}