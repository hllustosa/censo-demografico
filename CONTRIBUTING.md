# Contributing

## Development Setup

### Dev Container (recommended)

The project includes a [`.devcontainer/devcontainer.json`](.devcontainer/devcontainer.json) with:

- .NET 8 SDK
- Node.js 20
- Docker CLI (uses host Docker via socket — required for `make up` and Testcontainers)

**Steps:**

1. Open the repo in VS Code or Cursor
2. Run **Dev Containers: Reopen in Container**
3. Wait for `dotnet restore` (postCreateCommand)
4. Start the stack:

```bash
make up
make urls
```

### Local machine

**Requirements:** Docker, Docker Compose, Make, .NET 8 SDK (for tests)

```bash
make up      # start full stack
make test    # run tests
make down    # stop stack
```

Run `make help` for all available targets.

## Makefile Reference

| Target | Description |
|--------|-------------|
| `make help` | List all targets |
| `make env` | Create `.env` from `.env.example` if missing |
| `make up` | Start full Docker Compose stack |
| `make down` | Stop containers |
| `make restart` | Restart stack |
| `make logs` | Follow container logs |
| `make ps` | Show container status |
| `make build` | Build .NET solution |
| `make restore` | Restore NuGet packages |
| `make test` | Run all tests |
| `make test-unit` | Run unit tests |
| `make test-integration` | Run integration tests |
| `make lint` | Verify formatting |
| `make format` | Apply formatting |
| `make clean` | Stop containers and clean .NET artifacts |
| `make observability` | Start stack with Grafana/Jaeger/Prometheus |
| `make front-build` | Build React frontend |
| `make urls` | Print service URLs |
| `make seed-people` | Seed sample people via gateway |

## Configuration

Environment variables follow ASP.NET Core convention. Docker Compose sets them automatically for containerized services.

For local overrides, copy `.env.example` to `.env` (or run `make env`). Never commit `.env`. Demo values are **DEVELOPMENT ONLY**.

```bash
ConnectionStrings__DefaultConnection=mongodb://guest:guest@mongo:27017/?authSource=admin&replicaSet=rs0
RabbitMqConnection__HostName=rabbitmq
RabbitMqConnection__QueueName=statistics
Neo4j__Uri=bolt://neo4j:7687
```

## Testing

```bash
make test
make test-unit
make test-integration
```

Integration tests use [Testcontainers](https://dotnet.testcontainers.org/) — Docker must be running (available in the dev container via the host socket).

## Architecture Guidelines

- Each microservice owns its database; no cross-service DB access
- Integration events flow through RabbitMQ; **no HTTP between services**
- People publishes via transactional outbox; consumers must be idempotent
- See [docs/](docs/) and [docs/adr/](docs/adr/) for architecture documentation

## Pull Requests

1. Create a feature branch from `main`
2. Ensure `make test` passes
3. Update documentation if behavior or architecture changes
4. Keep changes focused and incremental
