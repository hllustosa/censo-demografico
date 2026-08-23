using System;
using System.Diagnostics;
using System.IO;

namespace Census.FamilyTree.Test.Integration
{
    internal static class Neo4jTestConfiguration
    {
        public static string ResolveUri()
        {
            var configured = Environment.GetEnvironmentVariable("NEO4J_URI");
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }

            var dockerIp = TryGetDockerNeo4jIp();
            if (!string.IsNullOrWhiteSpace(dockerIp))
            {
                return $"http://{dockerIp}:7474/db/data";
            }

            return "http://localhost:7474/db/data";
        }

        private static string? TryGetDockerNeo4jIp()
        {
            foreach (var dockerPath in ResolveDockerPaths())
            {
                try
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = dockerPath,
                        Arguments = "inspect neo4j --format {{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                    });

                    if (process == null)
                    {
                        continue;
                    }

                    var ip = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit(5000);
                    if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(ip))
                    {
                        return ip;
                    }
                }
                catch
                {
                    // Try the next docker binary path.
                }
            }

            return null;
        }

        private static string[] ResolveDockerPaths()
        {
            var paths = new[] { "docker", "/usr/bin/docker", "/usr/local/bin/docker" };
            foreach (var path in paths)
            {
                if (path == "docker" || File.Exists(path))
                {
                    return paths;
                }
            }

            return paths;
        }
    }
}
