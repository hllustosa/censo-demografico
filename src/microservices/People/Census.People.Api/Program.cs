using Census.People.Application.Behaviour;
using Census.People.Application.Services;
using Census.People.Domain.Interfaces;
using Census.People.Infra.Connection;
using Census.People.Infra.Outbox;
using Census.People.Infra.Repository;
using Census.People.Infra.Service;
using Census.Shared.Bus;
using Census.Shared.Bus.Implementation;
using Census.Shared.Bus.Interfaces;
using Census.Shared.Observability;
using Census.Shared.Web;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.AddCensusObservability("people-service");

builder.Services.AddServiceDefaults("people-service");
builder.Services.AddControllers();
builder.Services.AddCensusApiVersioning();
builder.Services.AddCensusOpenApi("People");
builder.Services.AddCensusAuthentication(builder.Configuration);
builder.Services.AddCensusRateLimiting();
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(Census.People.Application.Behaviour.ValidatorAssembly).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<Census.People.Application.Behaviour.ValidatorAssembly>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddTransient<IMongoConnection, MongoConnection>();
builder.Services.AddTransient<IGuidGenerator, GuidGenerator>();
builder.Services.AddTransient<IPersonRepository, PersonRepository>();
builder.Services.AddTransient<ITransactionManager, MongoTransactionManager>();
builder.Services.AddTransient<IOutboxStore, MongoOutboxStore>();
builder.Services.AddTransient<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
builder.Services.AddHostedService<OutboxProcessor>();
builder.Services.AddEventBus(builder.Configuration);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var mongoConnection = builder.Configuration.GetConnectionString("DefaultConnection")!;
var rabbitSection = builder.Configuration.GetSection("RabbitMqConnection");
var rabbitConnection = $"amqp://{rabbitSection["Username"]}:{rabbitSection["Password"]}@{rabbitSection["HostName"]}:5672";

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
    })
    .AddRabbitMQ(rabbitConnection, name: "rabbitmq");

var app = builder.Build();

app.UseCensusApiPipeline();
app.MapControllers().RequireRateLimiting(RateLimitingExtensions.GlobalPolicy);

app.Run();

public partial class Program;
