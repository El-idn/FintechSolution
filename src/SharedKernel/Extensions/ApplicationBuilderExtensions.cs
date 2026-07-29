using Microsoft.AspNetCore.Builder;
using SharedKernel.Middlewares;

namespace SharedKernel.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }

        //
        // public static IApplicationBuilder UseMyCustomMiddleware(this IApplicationBuilder app)
        // {
        //     return app.UseMiddleware<MyCustomMiddleware>();
        // }
    }
}
