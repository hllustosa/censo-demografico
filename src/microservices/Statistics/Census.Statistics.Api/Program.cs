using Census.Shared.Bus;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using Census.Shared.Observability;
using Census.Shared.Web;
using Census.Statistics.Api.Hubs;
using Census.Statistics.Api.Services;
using Census.Statistics.Application;
using Census.Statistics.Application.Behaviour;
using Census.Statistics.Application.Events;
using Census.Statistics.Domain.Interfaces;
using Census.Statistics.Infra.Connection;
using Census.Statistics.Infra.ProcessedEvents;
using Census.Statistics.Infra.Repository;
using Census.Statistics.Infra.Service;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.AddCensusObservability("statistics-service");

builder.Services.AddServiceDefaults("statistics-service");
builder.Services.AddControllers();
builder.Services.AddCensusApiVersioning();
builder.Services.AddCensusOpenApi("Statistics");
builder.Services.AddCensusAuthentication(builder.Configuration);
builder.Services.AddCensusRateLimiting();
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(BaseEventHandler).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<BaseEventHandler>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddTransient<IMongoConnection, MongoConnection>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
builder.Services.AddTransient<IGuidGenerator, GuidGenerator>();
builder.Services.AddTransient<ITransactionManager, MongoTransactionManager>();
builder.Services.AddTransient<IPersonCategoryRepository, PersonCategoryRepository>();
builder.Services.AddTransient<IPersonPerCityCounterRepository, PersonPerCityCounterRepository>();
builder.Services.AddTransient<IProcessedEventStore, MongoProcessedEventStore>();

builder.Services.AddTransient<PersonCreatedEventHandler>();
builder.Services.AddTransient<PersonDeletedEventHandler>();
builder.Services.AddTransient<PersonUpdatedEventHandler>();

builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);
builder.Services.AddScoped<INotificationSender, SignalRNotificationSender>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddEventBus(builder.Configuration);

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
app.MapHub<NotificationHub>("/hubs/notification");

var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<PersonCreatedEvent, PersonCreatedEventHandler>();
eventBus.Subscribe<PersonUpdatedEvent, PersonUpdatedEventHandler>();
eventBus.Subscribe<PersonDeletedEvent, PersonDeletedEventHandler>();

app.Run();

public partial class Program;
