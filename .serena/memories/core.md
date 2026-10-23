---
name: core
description: Top-level source map and project-wide invariants for the Schedule solution
metadata:
  type: project
---

# Schedule — .NET 10 Microservices Solution

Three microservices under `src/`:

| Service | Port | DB | Notes |
|---|---|---|---|
| `UserService/` | 5001 | PostgreSQL (EF Core) | gRPC server (`Grpc/Protos/userService.proto`) |
| `ScheduleService/` | 5002 | MongoDB | Keycloak JWT auth, Hangfire jobs, RabbitMQ consumer |
| `UserManagementService/` | 5003 | MongoDB | gRPC client to UserService, RabbitMQ publisher |

Shared projects:
- `src/ServiceDefaults/` — OpenTelemetry, health checks, service discovery (all services call `builder.AddServiceDefaults()`)
- `src/AppHost/` — .NET Aspire orchestration entry point

Each service follows Clean Architecture:
- `*.API` — controllers, middleware, DI wiring
- `Application` — MediatR handlers, validators, mappings
- `Domain` — entities, value objects, repository interfaces
- `DataAccess` — MongoDB/EF repos, Hangfire config
- `Infrastructure` — email, RabbitMQ clients

Key patterns: MediatR commands/queries, FluentValidation pipeline behavior, StyleCop enforced, xUnit+Moq+FluentAssertions tests.

## Frontend
Angular 19 SPA in `apps/web/` (own `.git`, not yet scaffolded). See `mem:frontend/core` for structure, stack, and feature memories.

## Workflow: Creating a Schedule
1. POST `/User` on UserManagementService → creates user
2. POST `/Department` → creates department
3. POST `/Department/addUserToDepartment` → auto-creates `UserScheduleRules` in ScheduleService via RabbitMQ
4. Set rules via ScheduleService, then POST `/Schedule/generate`
