# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this 
repository.

## Overview

This is a .NET 10 microservices solution for employee schedule management. It uses **.NET Aspire** for local orchestration, **Keycloak** for authentication, **RabbitMQ** for async messaging, **gRPC** for synchronous inter-service calls, and **Hangfire** for background job scheduling.

## Frontends — IMPORTANT: there are two, do NOT mix them up

The repo contains two separate frontend folders. Do not copy code, patterns, or conventions between them, and do not apply one's instructions to the other:

| Folder | Status | Stack | Notes |
|---|---|---|---|
| `apps/web/` | **NEW — active** | Angular | The current frontend. Has its own `CLAUDE.md` (and its own `.git`). Use this for new frontend work. |
| `legacy/electron-vue/` | **OLD — legacy** | (previous frontend) | Kept for reference only. Do not change unless explicitly asked. |

When working on frontend files, follow the `CLAUDE.md` inside that frontend's folder. The backend lives in `src/` (see below) and is the API contract for both.

## Running the Solution

**Preferred: via .NET Aspire (orchestrates all services + infra)**
```bash
dotnet run --project src/AppHost
```
This starts all three services, Keycloak, RabbitMQ, PostgreSQL, and MongoDB with service discovery wired automatically.

**Alternative: via Docker Compose**
```bash
docker-compose up --build
```

## Building & Testing

```bash
# Build entire solution
dotnet build Schedule.sln

# Run all tests
dotnet test Schedule.sln

# Run tests for a single project
dotnet test tests/UserServiceTests/UserServiceTests.csproj
dotnet test tests/ScheduleServiceTests/Application/Application.csproj
dotnet test tests/UserManagementTests/ClassLibrary3/UserManagementService.csproj

# EF Core migrations (UserService only — uses PostgreSQL)
dotnet ef migrations add <MigrationName> -s src/Services/UserService/UserService.API -p src/Services/UserService/Data
dotnet ef database update -s src/Services/UserService/UserService.API -p src/Services/UserService/Data
```

## Architecture

Three microservices, each following the same Clean Architecture layering:

| Layer | Project suffix |
|---|---|
| API (controllers, middleware, DI wiring) | `*.API` |
| Application (MediatR use cases, validators, mappings) | `Application` |
| Domain (entities, value objects) | `Domain` |
| Data/Infrastructure (DB, messaging, external services) | `DataAccess` / `Infrastructure` |

### Services

**UserService** (`src/Services/UserService/`) — port 5001
- Stores users in **PostgreSQL** (Entity Framework Core)
- Exposes a **gRPC server** (`Grpc/Protos/userService.proto`) consumed by UserManagementService
- Runs a seeder on startup (`IUserSeeder`)

**ScheduleService** (`src/Services/ScheduleService/`) — port 5002
- Stores schedules and `UserScheduleRule` records in **MongoDB**
- Authenticates via **Keycloak JWT** (realm: `schedule-service`, audience: `schedule-service`)
- Uses **Hangfire** for background schedule generation jobs
- Consumes RabbitMQ events from UserManagementService

**UserManagementService** (`src/Services/UserManagementService/`) — port 5003
- Stores departments and user-department memberships in **MongoDB**
- Calls UserService over **gRPC** (`GrpcClient/Protos/userService.proto`)
- Publishes events to **RabbitMQ** when users are added to departments
- A new `UserScheduleRule` record in ScheduleService's DB is created automatically when a user is added to a department

### Shared infrastructure

- **`Schedule.ServiceDefaults`** — shared Aspire defaults: OpenTelemetry (traces, metrics, logs), health checks, service discovery. Every service calls `builder.AddServiceDefaults()`.
- **`Schedule.AppHost`** — Aspire orchestration entry point; references all three service projects and provisions Keycloak, RabbitMQ, PostgreSQL, MongoDB.

### Key patterns

- **MediatR** for all command/query handling in Application layer
- **Hangfire** for recurring schedule-generation jobs (configured in Infrastructure)
- **StyleCop** analyzers enforced in all projects
- **`ExceptionHandlingMiddleware`** registered in every API pipeline (note: typo `ExeptionHadlingMiddleware` in code)
- Tests use **xUnit + Moq + FluentAssertions**

## Service Ports (docker-compose)

| Service | Port |
|---|---|
| UserService API | 5001 |
| ScheduleService API | 5002 |
| UserManagementService API | 5003 |
| RabbitMQ Management UI | 15672 |
| PostgreSQL | 5434 |
| MongoDB (schedule) | 27018 |
| MongoDB (management) | 27017 |
| pgAdmin | 5050 |
| Mongo Express (schedule) | 8082 |
| Mongo Express (management) | 8081 |

## Workflow: Creating a Schedule

1. POST `/User` on UserManagementService (port 5003) — creates a user
2. POST `/Department` on UserManagementService — creates a department
3. POST `/Department/addUserToDepartment` — links user to department; this auto-creates a `UserScheduleRule` in ScheduleService's MongoDB
4. Fill schedule rules via ScheduleService (port 5002)
5. Schedules can be created manually via POST `/Schedule` or generated automatically by Hangfire jobs
