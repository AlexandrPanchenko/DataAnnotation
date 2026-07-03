using JetFlight.Shared.UserContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JetFlight.WebApi.Authorization
{
    public class HasPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private Permission Permission { get; set; }

        private PermissionLevel Level { get; set; }

        public HasPermissionAttribute(Permission permission, PermissionLevel level)
        {
            Permission = permission;
            Level = level;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Items.TryGetValue("Permissions", out var value)
                && value is Dictionary<Permission, PermissionLevel> permissions
                && permissions.TryGetValue(Permission, out PermissionLevel level) && level >= Level)
            {
                return;
            }

            context.Result = new UnauthorizedResult();
        }
    }
}
