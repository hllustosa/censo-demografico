using System.Text.Json;
using Census.Shared.Observability;
using Census.Shared.Web.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Census.Shared.Web;

public static class ServiceDefaultsExtensions
{
    public static IServiceCollection AddServiceDefaults(this IServiceCollection services, string serviceName)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                EnrichProblemDetails(context.ProblemDetails, context.HttpContext, serviceName);
            };
        });

        services.AddHealthChecks();
        return services;
    }

    public static WebApplication MapServiceDefaults(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStatusCodePages(async statusContext =>
        {
            if (statusContext.HttpContext.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                await WriteProblemDetailsAsync(statusContext.HttpContext, new ProblemDetails
                {
                    Type = "https://censo.local/errors/unauthorized",
                    Title = "Não autenticado",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "É necessário estar autenticado para acessar este recurso."
                });
            }
            else if (statusContext.HttpContext.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                await WriteProblemDetailsAsync(statusContext.HttpContext, new ProblemDetails
                {
                    Type = "https://censo.local/errors/forbidden",
                    Title = "Acesso negado",
                    Status = StatusCodes.Status403Forbidden,
                    Detail = "Você não tem permissão para executar esta operação."
                });
            }
        });

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;

                if (exception is ValidationException validationException)
                {
                    var validationProblem = new ValidationProblemDetails(
                        validationException.Errors
                            .GroupBy(error => error.PropertyName)
                            .ToDictionary(
                                group => group.Key,
                                group => group.Select(error => error.ErrorMessage).ToArray()))
                    {
                        Type = "https://censo.local/errors/validation",
                        Title = "Falha na validação",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Corrija os campos indicados."
                    };

                    EnrichProblemDetails(validationProblem, context, context.RequestServices.GetRequiredService<IHostEnvironment>().ApplicationName);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(validationProblem);
                    return;
                }

                if (exception is CensusException censusException)
                {
                    var problem = new ProblemDetails
                    {
                        Type = censusException.ErrorType,
                        Title = GetTitleForStatus(censusException.StatusCode),
                        Status = censusException.StatusCode,
                        Detail = censusException.Message
                    };

                    EnrichProblemDetails(problem, context, context.RequestServices.GetRequiredService<IHostEnvironment>().ApplicationName);
                    context.Response.StatusCode = censusException.StatusCode;
                    await context.Response.WriteAsJsonAsync(problem);
                    return;
                }

                var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    Exception = exception,
                    ProblemDetails =
                    {
                        Type = "https://censo.local/errors/internal",
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Erro interno",
                        Detail = "Ocorreu um erro inesperado. Tente novamente mais tarde."
                    }
                });
            });
        });

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready");

        return app;
    }

    private static void EnrichProblemDetails(ProblemDetails problemDetails, HttpContext httpContext, string serviceName)
    {
        problemDetails.Extensions["service"] = serviceName;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["correlationId"] = httpContext.Response.Headers[CorrelationMiddlewareExtensions.CorrelationHeader].FirstOrDefault()
            ?? CorrelationContext.CorrelationId;
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static string GetTitleForStatus(int statusCode) => statusCode switch
    {
        StatusCodes.Status404NotFound => "Recurso não encontrado",
        StatusCodes.Status409Conflict => "Conflito",
        StatusCodes.Status403Forbidden => "Acesso negado",
        StatusCodes.Status422UnprocessableEntity => "Regra de negócio violada",
        _ => "Erro"
    };
}
