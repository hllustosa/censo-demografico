using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Census.Identity.Infra.Entities;
using Census.Shared.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Census.Identity.Infra;

public static class IdentityInfraExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Identity")
            ?? throw new InvalidOperationException("ConnectionStrings:Identity is required.");
        var databaseName = MongoUrl.Create(connectionString).DatabaseName ?? "identitydb";

        services.ConfigureMongoDbIdentity<ApplicationUser, ApplicationRole, Guid>(
                new MongoDbIdentityConfiguration
                {
                    MongoDbSettings = new MongoDbSettings
                    {
                        ConnectionString = connectionString,
                        DatabaseName = databaseName
                    },
                    IdentityOptionsAction = options =>
                    {
                        options.Password.RequiredLength = 8;
                        options.Password.RequireDigit = true;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireLowercase = true;
                        options.Password.RequireNonAlphanumeric = false;
                        options.User.RequireUniqueEmail = true;
                    }
                })
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });
        services.AddSingleton(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<RefreshToken>("refreshTokens"));

        services.AddHostedService<IdentityDataSeeder>();

        return services;
    }
}

public class IdentityDataSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<IdentityDataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in CensusRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
                _logger.LogInformation("Created role {Role}", role);
            }
        }

        var adminEmail = _configuration["Identity:Admin:Email"] ?? "admin@censo.local";
        var adminPassword = _configuration["Identity:Admin:Password"] ?? "Admin@12345";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Administrador",
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, CensusRoles.Admin);
                _logger.LogInformation("Created default admin user {Email}", adminEmail);
            }
            else
            {
                _logger.LogWarning("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
