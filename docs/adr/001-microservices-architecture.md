# Microservices architecture

## Status

Accepted

## Context

The project is a portfolio showcase for distributed systems. A modular monolith could implement the census domain with less operational cost.

## Decision

Use bounded-context microservices (Identity, People, Statistics, FamilyTree) communicating primarily through integration events, with an nginx edge gateway for the SPA.

## Consequences

Positive:

- Clear data ownership and independent persistence models
- Demonstrates eventual consistency, outbox, and polyglot persistence

Negative:

- Higher local ops cost (Compose, multiple images)
- Distributed debugging complexity

## Alternatives considered

- Modular monolith with in-process messaging
- Shared database across services (rejected: weakens boundaries)
