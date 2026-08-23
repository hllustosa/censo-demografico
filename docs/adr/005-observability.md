# Observability

## Status

Accepted

## Context

Distributed flows cannot be debugged with logs alone. Recruiters and engineers evaluating the portfolio need proof that telemetry is wired, not only that containers exist.

## Decision

Use OpenTelemetry for traces/metrics, Serilog JSON logs with correlation ids, Prometheus scrape endpoints, Grafana provisioning, and Jaeger via an OTel collector enabled by the Compose `observability` profile.

## Consequences

Positive:

- End-to-end visibility for HTTP → outbox → messaging → consumers
- Dashboards that show rates, latency, and failures

Negative:

- Extra containers and configuration surface
- Metric names must be kept honest and useful (avoid vanity metrics)

## Alternatives considered

- Vendor APM only
- Logs-only approach
