using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

namespace Census.Shared.Observability
{
    public static class ObservabilityExtensions
    {
        public static WebApplicationBuilder AddCensusObservability(this WebApplicationBuilder builder, string serviceName)
        {
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("service", serviceName)
                    .Enrich.WithThreadId()
                    .WriteTo.Console(new RenderedCompactJsonFormatter());
            });

            var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource(serviceName);

                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                    }
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddMeter(EventBusMetrics.MeterName)
                        .AddPrometheusExporter();

                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                    }
                });

            return builder;
        }

        public static WebApplication UseCensusObservability(this WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, _) =>
                {
                    diagnosticContext.Set("CorrelationId", CorrelationContext.CorrelationId);
                };
            });

            app.MapPrometheusScrapingEndpoint("/metrics");
            return app;
        }
    }
}
