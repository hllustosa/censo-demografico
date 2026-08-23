# Event-driven communication

## Status

Accepted

## Context

Statistics and FamilyTree need updates when people change. Synchronous HTTP between services couples availability and creates chatty read-path repairs.

## Decision

People publishes integration events (`PersonCreated` / `Updated` / `Deleted`) via RabbitMQ. Downstream services own their projections. FamilyTree does not call People over HTTP.

## Consequences

Positive:

- Loose coupling and independent deployability of read models
- Natural fit for eventual consistency demos

Negative:

- Temporary inconsistency windows
- Requires idempotent consumers and failure handling (retry/DLQ)

## Alternatives considered

- Synchronous orchestration / saga over HTTP
- Shared database triggers
