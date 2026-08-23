using Census.Identity.Api.Services;
using Census.Identity.Infra;
using Census.Shared.Observability;
using Census.Shared.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddCensusObservability("identity-service");

builder.Services.AddServiceDefaults("identity-service");
builder.Services.AddControllers();
builder.Services.AddCensusApiVersioning();
builder.Services.AddCensusOpenApi("Identity");
builder.Services.AddCensusAuthentication(builder.Configuration);
builder.Services.AddCensusRateLimiting();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var mongoConnection = builder.Configuration.GetConnectionString("Identity")!;
builder.Services.AddHealthChecks()
    .AddCheck("mongodb", () =>
    {
        try
        {
            var client = new MongoDB.Driver.MongoClient(mongoConnection);
            client.GetDatabase("admin").RunCommand<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(ex.Message);
        }
    });

var app = builder.Build();

app.UseCensusApiPipeline();
app.MapControllers().RequireRateLimiting(RateLimitingExtensions.GlobalPolicy);

app.Run();

public partial class Program;
