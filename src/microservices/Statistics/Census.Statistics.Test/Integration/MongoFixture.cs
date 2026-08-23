using System.Diagnostics;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Census.Statistics.Test.Integration;

/// <summary>
/// Single-node Mongo replica set for Statistics integration tests.
/// Plain container + log wait + bridge IP for nested Docker / DinD.
/// </summary>
public sealed class MongoFixture : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder()
        .WithImage("mongo:6.0.16")
        .WithCommand("--replSet", "rs0", "--bind_ip_all")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilMessageIsLogged("Waiting for connections"))
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
        await _container.StartAsync();

        var memberHost = ResolveMemberHost();
        var direct = $"mongodb://{memberHost}/?directConnection=true";
        var client = new MongoClient(direct);

        try
        {
            await client.GetDatabase("admin").RunCommandAsync<BsonDocument>(
                new BsonDocument
                {
                    {
                        "replSetInitiate",
                        new BsonDocument
                        {
                            { "_id", "rs0" },
                            {
                                "members",
                                new BsonArray
                                {
                                    new BsonDocument { { "_id", 0 }, { "host", memberHost } }
                                }
                            }
                        }
                    }
                });
        }
        catch (MongoCommandException)
        {
        }

        var primaryReady = false;
        for (var i = 0; i < 40; i++)
        {
            try
            {
                var hello = await client.GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("hello", 1));
                if (hello.GetValue("isWritablePrimary", false).ToBoolean() || hello.GetValue("ismaster", false).ToBoolean())
                {
                    primaryReady = true;
                    break;
                }
            }
            catch
            {
            }

            await Task.Delay(500);
        }

        if (!primaryReady)
        {
            throw new InvalidOperationException(
                $"Mongo replica set did not become primary at {memberHost}. Check Docker networking.");
        }

        ConnectionString = $"mongodb://{memberHost}/?replicaSet=rs0";
    }

    public async Task DisposeAsync()
    {
        try
        {
            await _container.DisposeAsync();
        }
        catch
        {
        }
    }

    private string ResolveMemberHost()
    {
        var bridgeIp = TryGetContainerIp(_container.Id);
        if (!string.IsNullOrWhiteSpace(bridgeIp))
        {
            return $"{bridgeIp}:27017";
        }

        return $"{_container.Hostname}:{_container.GetMappedPublicPort(27017)}";
    }

    private static string? TryGetContainerIp(string containerId)
    {
        try
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
            });

            if (process is null)
            {
                return null;
            }

            var ip = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(5000);
            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(ip) ? ip : null;
        }
        catch
        {
            return null;
        }
    }
}

[CollectionDefinition("StatisticsIntegration")]
public class StatisticsIntegrationCollection : ICollectionFixture<MongoFixture>
{
}
