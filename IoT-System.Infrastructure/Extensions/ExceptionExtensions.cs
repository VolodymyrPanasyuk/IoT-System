using IoT_System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace IoT_System.Infrastructure.Extensions;

public static class ExceptionExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}