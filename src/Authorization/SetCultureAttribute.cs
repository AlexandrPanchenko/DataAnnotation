using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;

namespace JetFlight.WebApi.Authorization
{
    public class SetCultureAttribute : ActionFilterAttribute
    {
        private readonly string _culture;

        public SetCultureAttribute(string culture)
        {
            _culture = culture;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            CultureInfo.CurrentCulture = new CultureInfo(_culture);
            CultureInfo.CurrentUICulture = new CultureInfo(_culture);
            base.OnActionExecuting(context);
        }
    }
}
