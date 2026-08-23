using Census.FamilyTree.Application.Behaviour;
using Census.FamilyTree.Application.Events;
using Census.FamilyTree.Application.Queries;
using Census.FamilyTree.Application.Validation;
using Census.FamilyTree.Domain.Repository;
using Census.FamilyTree.Infra.Connection;
using Census.FamilyTree.Infra.ProcessedEvents;
using Census.FamilyTree.Infra.Repository;
using Census.Shared.Bus;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using Census.Shared.Observability;
using Census.Shared.Web;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.AddCensusObservability("familytree-service");

builder.Services.AddServiceDefaults("familytree-service");
builder.Services.AddControllers();
builder.Services.AddCensusApiVersioning();
builder.Services.AddCensusOpenApi("FamilyTree");
builder.Services.AddCensusAuthentication(builder.Configuration);
builder.Services.AddCensusRateLimiting();
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(FamilyTreeQuery).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<FamilyTreeQueryValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

builder.Services.AddTransient<INeo4jConnection, Neo4jConnection>();
builder.Services.AddTransient<IPersonFamilyTreeRepository, PersonFamilyTreeRepository>();
builder.Services.AddTransient<IProcessedEventStore, Neo4jProcessedEventStore>();

builder.Services.AddTransient<PersonCreatedEventHandler>();
builder.Services.AddTransient<PersonDeletedEventHandler>();
builder.Services.AddTransient<PersonUpdatedEventHandler>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddEventBus(builder.Configuration);

var rabbitSection = builder.Configuration.GetSection("RabbitMqConnection");
var rabbitConnection = $"amqp://{rabbitSection["Username"]}:{rabbitSection["Password"]}@{rabbitSection["HostName"]}:5672";

builder.Services.AddHealthChecks()
    .AddCheck("neo4j", () =>
    {
        try
        {
            var connection = new Neo4jConnection(builder.Configuration);
            connection.GetClient().GetAwaiter().GetResult();
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

var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<PersonCreatedEvent, PersonCreatedEventHandler>();
eventBus.Subscribe<PersonUpdatedEvent, PersonUpdatedEventHandler>();
eventBus.Subscribe<PersonDeletedEvent, PersonDeletedEventHandler>();

app.Run();

public partial class Program;
