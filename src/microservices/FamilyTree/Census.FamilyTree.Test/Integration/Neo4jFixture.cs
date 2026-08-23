using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Testcontainers.Neo4j;
using Xunit;

namespace Census.FamilyTree.Test.Integration
{
    public sealed class Neo4jFixture : IAsyncLifetime
    {
        private Neo4jContainer? _container;

        public string BoltUri { get; private set; } = "bolt://localhost:7687";

        public string Username { get; private set; } = "neo4j";

        public string Password { get; private set; } = "testtest";

        public async Task InitializeAsync()
        {
            var configuredUri = Environment.GetEnvironmentVariable("NEO4J_URI");
            if (!string.IsNullOrWhiteSpace(configuredUri))
            {
                BoltUri = configuredUri;
                Username = Environment.GetEnvironmentVariable("NEO4J_USERNAME") ?? Username;
                Password = Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? Password;
                return;
            }

            Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

            // Default Neo4j wait hits mapped localhost HTTP ports, which often fail in nested Docker.
            // Log-based readiness uses the Docker API and works with bridge-IP Bolt access.
            _container = new Neo4jBuilder()
                .WithImage("neo4j:5.26.0")
                .WithEnvironment("NEO4J_AUTH", "neo4j/testtest")
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilMessageIsLogged("Bolt enabled on"))
                .Build();

            await _container.StartAsync();

            // Prefer the container bridge IP. Mapped localhost ports often fail in nested Docker.
            var bridgeIp = TryGetContainerIp(_container.Id);
            BoltUri = !string.IsNullOrWhiteSpace(bridgeIp)
                ? $"bolt://{bridgeIp}:7687"
                : _container.GetConnectionString();
        }

        public async Task DisposeAsync()
        {
            if (_container is null)
            {
                return;
            }

            try
            {
                await _container.DisposeAsync();
            }
            catch
            {
                // Ignore cleanup failures in constrained Docker environments.
            }
            finally
            {
                _container = null;
            }
        }

        public IReadOnlyDictionary<string, string?> ConfigurationOverrides() =>
            new Dictionary<string, string?>
            {
                ["Neo4j:Uri"] = BoltUri,
                ["Neo4j:Username"] = Username,
                ["Neo4j:Password"] = Password,
            };

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

    [CollectionDefinition("Neo4jIntegration")]
    public class Neo4jIntegrationCollection : ICollectionFixture<Neo4jFixture>
    {
    }
}
