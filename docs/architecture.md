# Architecture

Distributed systems and software architecture showcase implemented through a demographic census domain.

> Microservices are intentionally used in this project as a learning and architectural demonstration environment. In a real production system, this architecture should only be adopted when justified by organizational, operational, scalability and deployment requirements.

## System context

```text
                     ┌──────────────┐
                     │  React SPA   │
                     └──────┬───────┘
                            │
                     ┌──────▼───────┐
                     │ nginx Gateway│
                     │    :8080     │
                     └──────┬───────┘
          ┌─────────────────┼─────────────────┬──────────────┐
          │                 │                 │              │
   ┌──────▼──────┐   ┌──────▼──────┐   ┌─────▼────────┐ ┌──▼───────┐
   │   People    │   │ Statistics  │   │  FamilyTree  │ │ Identity │
   └──────┬──────┘   └──────┬──────┘   └─────┬────────┘ └──┬───────┘
          │                 │                 │              │
        MongoDB           MongoDB           Neo4j 5.x      MongoDB
          │
          ▼
       Outbox → RabbitMQ → Integration Events
```

## Service ownership

| Service | Responsibility | Owned data | Database | Public API | Produced events | Consumed events |
|---------|----------------|------------|----------|------------|-----------------|-----------------|
| Identity | Auth, users, JWT | Users/roles | MongoDB | `/auth/*` | — | — |
| People | Citizen CRUD (source of truth) | People | MongoDB | `/person/*` | PersonCreated/Updated/Deleted | — |
| Statistics | Aggregated counters + SignalR | Counters | MongoDB | `/stats/*` | — | Person* |
| FamilyTree | Genealogy graph queries | Graph nodes/edges | Neo4j | `/family/*` | — | Person* |

## Consistency model

People is the source of truth. Statistics and FamilyTree are **eventually consistent** read models updated by RabbitMQ integration events published via a **transactional outbox**.

## Communication rules

- Synchronous HTTP from browser → nginx → services only.
- No service-to-service HTTP for domain sync (FamilyTree does not call People).
- Asynchronous integration events over RabbitMQ for cross-context updates.

## Shared libraries

- `Census.Contracts` — versioned integration event payloads (no domain logic).
- `Census.Shared` — technical cross-cutting concerns (messaging, auth middleware, observability, web defaults).
- `Census.Testing` — test helpers only (not shipped in production images).

See [ADRs](adr/) for decision records and [events.md](events.md) for the event catalog.
