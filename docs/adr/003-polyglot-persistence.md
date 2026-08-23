# Polyglot persistence

## Status

Accepted

## Context

Citizen records are document-oriented. Family relationships are naturally a graph. Aggregates for dashboards are counter documents.

## Decision

- MongoDB for People, Statistics, and Identity
- Neo4j 5.x (Bolt) for FamilyTree

## Consequences

Positive:

- Storage model matches access patterns
- Strong teaching signal for polyglot persistence

Negative:

- More operational knowledge required
- Cross-store consistency is eventual only

## Alternatives considered

- Relational single store with recursive CTEs for trees
- Mongo-only graph emulation
