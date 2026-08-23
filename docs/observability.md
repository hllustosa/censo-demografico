# Observability

## Goals

Make a single user action (create person) traceable across:

```text
HTTP (People) → Mongo (person + outbox) → OutboxProcessor → RabbitMQ
  → Statistics consumer → Mongo
  → FamilyTree consumer → Neo4j
```

## Stack

| Component | Role |
|-----------|------|
| OpenTelemetry SDK | Traces + metrics in each API |
| OTel Collector | Receives OTLP, forwards to Jaeger/Prometheus |
| Jaeger | Distributed traces UI |
| Prometheus | Scrapes `/metrics` |
| Grafana | Dashboards |
| Serilog | Structured JSON logs with correlation id |

## Enabling export

`make observability` injects:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
```

Without that variable, apps still expose Prometheus `/metrics` but do not export OTLP traces.

## Useful metrics

| Metric | Meaning |
|--------|---------|
| HTTP request duration histograms | Latency and error rates |
| `census.messages.*` | Publish/consume/retry/DLQ/duplicate |
| `outbox_messages_published_total` | Outbox successes |
| `outbox_messages_failed_total` | Outbox publish failures / poison |

## Dashboards

Provisioned dashboard: **Census Platform Overview** (Grafana folder Census).

Capture screenshots after a seed run and store under `docs/assets/` if desired.

## How to follow a request

1. Create a person via the UI or API.
2. Open Jaeger → service `people-service` → find the HTTP span.
3. Follow child spans / linked messaging work into Statistics and FamilyTree when instrumented.
4. Cross-check Grafana panels for publish/consume rates and outbox failures.
5. Logs include `CorrelationId` for join across services.
