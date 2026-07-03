using JetFlight.Shared.UserContext;

namespace JetFlight.WebApi.Helpers
{
    public static class UserContextExtensions
    {
        public static IServiceCollection AddUserContext(this IServiceCollection services)
        {
            services.AddScoped<IUserContext>(serviceProvider =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor.HttpContext;

                var userContext = new UserContext();

                if (httpContext?.User?.Identity?.IsAuthenticated != true)
                {
                    return userContext;
                }

                if (httpContext.User.IsInRole(UserRole.Admin.ToString()))
                {
                    var idClaim = httpContext.User.Claims.First(x => x.Type == "id").Value;
                    var userId = int.Parse(idClaim);
                    userContext.AdminId = userId;
                }

                if (httpContext.User.IsInRole(UserRole.Customer.ToString()))
                {
                    var idClaim = httpContext.User.Claims.First(x => x.Type == "id").Value;
                    var userId = int.Parse(idClaim);
                    userContext.CustomerId = userId;
                    var branchClaim = httpContext.User.Claims.First(x => x.Type == "branchId").Value;
                    userContext.BranchId = Enum.Parse<Shared.Models.Store.Branches>(branchClaim);
                }

                if (httpContext.User.IsInRole(UserRole.Cashdesk.ToString()))
                {
                    var clientId = httpContext.User.Claims.First(x => x.Type == "clientId").Value;
                    userContext.ClientId = clientId;
                }

                return userContext;
            });

            return services;
        }
    }
}
