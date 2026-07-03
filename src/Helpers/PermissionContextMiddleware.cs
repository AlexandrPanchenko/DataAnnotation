using JetFlight.Service.Services;
using JetFlight.Shared.UserContext;
using System.Net;

namespace JetFlight.WebApi.Helpers;

public class PermissionContextMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserContext userContext, ICustomerService customerService, IAuthorizeService authorizeService)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            if (context.User.IsInRole(UserRole.Admin))
            {
                try
                {
                    var permissions = await authorizeService.GetAdminPermissions(userContext.AdminId.Value);
                    context.Items["Permissions"] = permissions;
                }
                catch (ArgumentException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
            }
            
            if (context.User.IsInRole(UserRole.Customer))
            {
                var numberCheck = await customerService.IsNumberMatched(context.User.Claims.First(x => x.Type == "phone_number").Value);
                if (!numberCheck)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
            }

            if (context.User.IsInRole(UserRole.Cashdesk) && userContext.ClientId != Environment.GetEnvironmentVariable("CASHDESK_CLIENT_ID")!)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
        }

        await _next(context);
    }

}
