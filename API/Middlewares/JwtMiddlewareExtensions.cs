using ResumableFileTransfer.API.Models;
using Microsoft.AspNetCore.Builder;

namespace ResumableFileTransfer.API.Middlewares
{
    public static class JwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuth(
        this IApplicationBuilder builder, JwtOption jwtOption)
        {
            return builder.UseMiddleware<JwtMiddleware>(jwtOption);
        }
    }
}
