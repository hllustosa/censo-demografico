using Census.Shared.Observability;
using Microsoft.AspNetCore.Builder;

namespace Census.Shared.Web;

public static class CensusApiPipelineExtensions
{
    public static WebApplication UseCensusApiPipeline(this WebApplication app)
    {
        app.UseCorrelationId();
        app.UseCors();
        app.UseCensusRateLimiting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapServiceDefaults();
        app.UseCensusObservability();
        app.MapCensusOpenApi();

        return app;
    }
}
