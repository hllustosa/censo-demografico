# API gateway

## Status

Accepted

## Context

The SPA must not hardcode multiple service origins in production. Options included YARP, Ocelot, or nginx reverse proxy already used to serve the SPA.

## Decision

Keep **nginx** as the edge gateway (path routing to Identity/People/FamilyTree/Statistics). Do not add YARP solely for optics.

## Consequences

Positive:

- One public port (`8080`)
- Simple, well-understood edge for a Compose demo
- SPA remains gateway-first

Negative:

- Cross-cutting auth/rate-limit aggregation stays mostly in each service
- No .NET-native gateway customization

## Alternatives considered

- YARP BFF (rejected for this scope: nginx already solves routing)
- Exposing each microservice port to the browser (rejected)
