using JetFlight.Shared.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace JetFlight.WebApi.Controllers
{
    public class BaseController : ControllerBase
    {
        protected ErrorResponseModel CreateErrorResponseModel(IList<string> errorList)
        {
            return new ErrorResponseModel()
            {
                ErrorMessage = errorList
                    .Distinct()
                    .ToList()
            };
        }
    }
}