using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Census.Identity.Api.Contracts;
using Census.Identity.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Testcontainers.MongoDb;
using Xunit;

namespace Census.Identity.Test;

public class AuthIntegrationTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder().Build();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Identity"] = _mongoContainer.GetConnectionString() + "identitydb?authSource=admin",
                        ["Jwt:SigningKey"] = "CensusDevSigningKeyMustBeAtLeast32CharsLong!",
                        ["Jwt:Issuer"] = "census-identity",
                        ["Jwt:Audience"] = "census-api",
                        ["Identity:Admin:Email"] = "admin@censo.local",
                        ["Identity:Admin:Password"] = "Admin@12345"
                    });
                });
            });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
        await seeder.StartAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var content = new StringContent(
            JsonConvert.SerializeObject(new LoginRequest("admin@censo.local", "Admin@12345")),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/v1/auth/login", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = JsonConvert.DeserializeObject<AuthResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(body?.AccessToken);
        Assert.Contains("Admin", body.User.Roles);
    }

    [Fact]
    public async Task Users_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Users_WithAdminToken_ReturnsOk()
    {
        var loginContent = new StringContent(
            JsonConvert.SerializeObject(new LoginRequest("admin@censo.local", "Admin@12345")),
            Encoding.UTF8,
            "application/json");
        var loginResponse = await _client.PostAsync("/api/v1/auth/login", loginContent);
        var auth = JsonConvert.DeserializeObject<AuthResponse>(await loginResponse.Content.ReadAsStringAsync());

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _mongoContainer.DisposeAsync();
    }
}
