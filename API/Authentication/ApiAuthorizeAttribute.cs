using ResumableFileTransfer.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace ResumableFileTransfer.API.Authentication
{
    public class ApiAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var softwareToken = CommonHelper.GetValueFromPayload(context.HttpContext, Consts.ENCRYPTED_SOFTWARE_TOKEN);
            // a 401 code will be returned only if jwt token validation failed
            // in other case, a 403 code will be returned

            if (softwareToken == null)
            {
                context.Result = new UnauthorizedObjectResult(new { Message = "Jwt token is invalid" });
            }
        }
    }
}
