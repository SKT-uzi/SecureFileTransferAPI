using Microsoft.AspNetCore.Builder;

namespace ResumableFileTransfer.API.Middlewares
{
    public static class ErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandler(
        this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
