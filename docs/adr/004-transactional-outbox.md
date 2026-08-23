# Transactional outbox

## Status

Accepted

## Context

Publishing to RabbitMQ in the same request as a Mongo write creates a dual-write problem: the database can commit while the broker publish fails (or vice versa).

## Decision

Persist the domain change and an outbox message in **one MongoDB transaction** (replica set required). A background `OutboxProcessor` claims leased messages, publishes to RabbitMQ, then marks them published. Poison/unknown types are marked failed.

This project intentionally demonstrates the Transactional Outbox Pattern to address the dual-write problem.

## Consequences

Positive:

- At-least-once publication aligned with committed state
- Retryable publish without losing the business write

Negative:

- Requires Mongo replica set even for local single-node demos
- Introduces publish lag (seconds)

## Alternatives considered

- Direct publish after save (rejected: dual-write)
- Change data capture / Debezium (heavier than needed here)
