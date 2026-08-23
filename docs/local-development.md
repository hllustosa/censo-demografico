# Local development

## Prerequisites

- Docker + Docker Compose
- .NET 8 SDK
- Node.js 20+ (frontend)
- Make (optional; commands map 1:1 to `dotnet` / `docker compose`)

Windows without Make: run the underlying commands from the Makefile targets.

## Quick start

```bash
cp .env.example .env   # or: make env
make up                # builds and starts the stack
make urls              # prints entrypoints
```

Open http://localhost:8080

Demo admin credentials are in `.env` (`Identity__Admin__*`). **DEVELOPMENT ONLY.**

## Observability stack

```bash
make observability
```

This sets `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317` and starts Grafana, Prometheus, Jaeger, and the OTel collector.

## Common commands

| Command | Purpose |
|---------|---------|
| `make build` | Build .NET solution |
| `make test` | All tests |
| `make test-unit` | Unit tests |
| `make test-integration` | Integration tests (Testcontainers) |
| `make lint` | `dotnet format --verify-no-changes` |
| `make format` | Apply formatting |
| `make logs` | Follow compose logs |
| `make down` | Stop stack |
| `make seed-people` | Seed sample people |

From a **Dev Container**, Compose ports bind on the Docker host. If
`http://localhost:8080` fails from the workspace shell, use
`http://host.docker.internal:8080` (the seed script probes this automatically).

## Configuration notes

Local Mongo uses an **unauthenticated single-node replica set** so the transactional outbox works without keyFile complexity. Production should enable auth + keyFile / TLS.

## Ports exposed to the host

| Port | Service |
|------|---------|
| 8080 | Frontend + nginx gateway (primary entry) |
| 15672 | RabbitMQ Management (dev) |
| 7474 | Neo4j Browser (dev) |
| 3000 / 9090 / 16686 | Grafana / Prometheus / Jaeger (observability profile) |

MongoDB, Bolt, AMQP, and microservice HTTP ports are **not** published to the host by default. Reach APIs through the gateway.
