using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Census.Identity.Api.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Testcontainers.MongoDb;
using Xunit;

namespace Census.Identity.Test;

public sealed class IdentityMongoFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:6.0.16")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
        await _mongoContainer.StartAsync();

        var connectionString = BuildIdentityConnectionString(_mongoContainer);

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("ConnectionStrings:Identity", connectionString);
                builder.UseSetting("Jwt:SigningKey", "CensusDevSigningKeyMustBeAtLeast32CharsLong!");
                builder.UseSetting("Jwt:Issuer", "census-identity");
                builder.UseSetting("Jwt:Audience", "census-api");
                builder.UseSetting("Identity:Admin:Email", "admin@censo.local");
                builder.UseSetting("Identity:Admin:Password", "Admin@12345");
            });

        // Force host start + IdentityDataSeeder before tests run.
        using var client = Factory.CreateClient();
        _ = client;
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        try
        {
            await _mongoContainer.DisposeAsync();
        }
        catch
        {
        }
    }

    private static string BuildIdentityConnectionString(MongoDbContainer container)
    {
        var bridgeIp = TryGetContainerIp(container.Id)
            ?? throw new InvalidOperationException(
                $"Could not resolve Docker bridge IP for container {container.Id}. GetConnectionString={container.GetConnectionString()}");

        // Testcontainers.MongoDb enables auth by default (mongo/mongo).
        return $"mongodb://mongo:mongo@{bridgeIp}:27017/identitydb?authSource=admin";
    }

    private static string? TryGetContainerIp(string containerId)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "docker",
            ArgumentList =
            {
                "inspect",
                "-f",
                "{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}",
                containerId
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        }) ?? throw new InvalidOperationException("Failed to start docker inspect.");

        var ip = process.StandardOutput.ReadToEnd().Trim();
        var err = process.StandardError.ReadToEnd().Trim();
        process.WaitForExit(5000);
        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(ip))
        {
            throw new InvalidOperationException($"docker inspect failed ({process.ExitCode}): {err}");
        }

        return ip;
    }
}

[CollectionDefinition("IdentityIntegration")]
public class IdentityIntegrationCollection : ICollectionFixture<IdentityMongoFixture>
{
}

[Collection("IdentityIntegration")]
public class AuthIntegrationTests
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(IdentityMongoFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
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
}
