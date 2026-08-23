using Census.Shared.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Census.Shared.Observability
{
    public static class CorrelationMiddlewareExtensions
    {
        public const string CorrelationHeader = "X-Correlation-Id";

        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault()
                    ?? CorrelationContext.EnsureCorrelationId();

                CorrelationContext.CorrelationId = correlationId;
                context.Response.Headers[CorrelationHeader] = correlationId;

                await next();
            });
        }
    }
}
