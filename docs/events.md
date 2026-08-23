# Integration events

Events are **integration contracts**, not internal domain entities. Canonical types live in `Census.Contracts`.

## Delivery guarantees

RabbitMQ consumers assume **at-least-once delivery**.

Duplicates can happen when:

- a consumer processes a message then crashes before ACK;
- an outbox publish is retried after a broker timeout;
- a lease expires and another outbox worker republishes.

Consumers track processed event IDs (`IProcessedEventStore`) so duplicate deliveries are skipped.

## Failure handling

```text
Normal flow → handler succeeds → ACK

Failure → retry (bounded, with retry count header)
Failure after max retries → Dead Letter Queue ({queue}.dlq)
```

DLQ messages are observable via RabbitMQ Management. Reprocessing is a deliberate ops action (not automatic in this demo).

---

## PersonCreated

| Field | Value |
|-------|-------|
| Producer | People |
| Consumers | Statistics, FamilyTree |
| Purpose | Create read-model rows / graph node |
| Payload | `PersonDTO` (id, name, demographics, parents) |
| Version | 1 (type name `PersonCreatedEvent`) |
| Idempotency | ProcessedEvent store by event `Id` |
| Failure handling | Retry then DLQ |

## PersonUpdated

| Field | Value |
|-------|-------|
| Producer | People |
| Consumers | Statistics, FamilyTree |
| Purpose | Adjust counters / update graph relationships |
| Payload | `OldPersonData`, `NewPersonData` |
| Version | 1 |
| Idempotency | ProcessedEvent store by event `Id` |
| Failure handling | Retry then DLQ |

## PersonDeleted

| Field | Value |
|-------|-------|
| Producer | People |
| Consumers | Statistics, FamilyTree |
| Purpose | Decrement counters / remove graph node |
| Payload | `PersonDTO` |
| Version | 1 |
| Idempotency | ProcessedEvent store by event `Id` |
| Failure handling | Retry then DLQ |
