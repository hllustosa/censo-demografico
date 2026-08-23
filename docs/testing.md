# Testing

## Layout

| Kind | Location | Notes |
|------|----------|-------|
| Unit | `*/Test/Unit` | Handlers, validators, business rules |
| Integration | `*/Test/Integration` | APIs + Testcontainers (Mongo replica set, Neo4j 5.x) |
| Architecture | `Census.Architecture.Tests` | NetArchTest layering / boundary rules |

## Commands

```bash
make test
make test-unit
make test-integration
```

Integration tests require Docker (Testcontainers).

In nested Docker (VS Code Dev Containers + Docker socket), fixtures prefer the
container **bridge IP** and **log-based readiness** instead of mapped `localhost`
ports, which are often unreachable from the workspace container. Set
`TESTCONTAINERS_RYUK_DISABLED=true` if Ryuk cleanup fails in constrained hosts.

## Architecture rules enforced

- People Domain must not depend on Application, Infra, or Api
- People Application must not depend on Infra or Api
- Statistics / FamilyTree Domain must not depend on People internals

## What we intentionally do not over-test

- Framework wiring details
- Pure DTO mapping without rules
- Infinite mock chains with no behavior under test
