# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this 
repository.

## Overview

This is a .NET 10 microservices solution for employee schedule management. It uses **.NET Aspire** for local orchestration, **Keycloak** for authentication, **RabbitMQ** for async messaging and RPC, and **Hangfire** for background job scheduling.

## Frontends — IMPORTANT: there are two, do NOT mix them up

The repo contains two separate frontend folders. Do not copy code, patterns, or conventions between them, and do not apply one's instructions to the other:

| Folder | Status | Stack | Notes |
|---|---|---|---|
| `../scheduleFe/` | **NEW — active** | Angular | The current frontend. Has its own `AGENTS.md` (and its own `.git`). Use this for new frontend work. |
| `../front/electron-vue/` | **OLD — legacy** | (previous frontend) | Kept for reference only. Do not change unless explicitly asked. |

When working on frontend files, follow the `AGENTS.md` inside that frontend's folder. The backend lives in `src/` (see below) and is the API contract for both.

## Running the Solution

**Preferred: via .NET Aspire (orchestrates all services + infra)**
```bash
dotnet run --project src/AppHost
```
This starts both services, Keycloak, RabbitMQ, and PostgreSQL with service discovery wired automatically.

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
dotnet test tests/ScheduleServiceTests/Application/Application.csproj
dotnet test tests/UserManagementTests/ClassLibrary3/UserManagementService.csproj
```

## Architecture

Two microservices, each following the same Clean Architecture layering:

| Layer | Project suffix |
|---|---|
| API (controllers, middleware, DI wiring) | `*.API` |
| Application (MediatR use cases, validators, mappings) | `Application` |
| Domain (entities, value objects) | `Domain` |
| Data/Infrastructure (DB, messaging, external services) | `DataAccess` / `Infrastructure` |

### Services

**ScheduleService** (`src/Services/ScheduleService/`) — port 5002
- Stores monthly schedules, shifts, generation rules and calendar exceptions in **PostgreSQL**, schema `schedule`
- Authenticates via **Keycloak JWT** (realm: `schedule-service`, audience: `schedule-service`)
- Uses **Hangfire** for background schedule generation jobs
- Consumes RabbitMQ events from UserManagementService

**UserManagementService** (`src/Services/UserManagementService/`) — port 5003
- Stores users, departments, clinics and user-department memberships in **PostgreSQL**, schema `user_management`
- Manages authentication users and permissions through the **Keycloak Admin API**
- Publishes events to **RabbitMQ** when users are added to departments
- A monthly schedule and generation rule in ScheduleService's DB is created automatically when a user is added to a department

### Shared infrastructure

- **`Schedule.ServiceDefaults`** — shared Aspire defaults: OpenTelemetry (traces, metrics, logs), health checks, service discovery. Every service calls `builder.AddServiceDefaults()`.
- **`Schedule.AppHost`** — Aspire orchestration entry point; references both active service projects and provisions Keycloak, RabbitMQ, and PostgreSQL.

### Key patterns

- **MediatR** for all command/query handling in Application layer
- **Hangfire** on PostgreSQL: isolated `schedule_hangfire` and `user_management_hangfire` schemas
- **StyleCop** analyzers enforced in all projects
- **`ExceptionHandlingMiddleware`** registered in every API pipeline (note: typo `ExeptionHadlingMiddleware` in code)
- Tests use **xUnit + Moq + FluentAssertions**

## Service Ports (docker-compose)

| Service | Port |
|---|---|
| ScheduleService API | 5002 |
| UserManagementService API | 5003 |
| RabbitMQ Management UI | 15672 |
| PostgreSQL (application_db) | 5433 |
| pgAdmin | 5050 |

## Workflow: Creating a Schedule

1. POST `/User` on UserManagementService (port 5003) — creates a user
2. POST `/Department` on UserManagementService — creates a department
3. POST `/Department/addUserToDepartment` — links user to department; this creates a monthly schedule and generation rule in ScheduleService's PostgreSQL schema
4. Fill schedule rules via ScheduleService (port 5002)
5. Schedules can be created manually via POST `/Schedule` or generated automatically by Hangfire jobs

## PostgreSQL storage and transition

Both APIs use `ConnectionStrings__applicationDb`, with independent EF contexts and migration histories in their own schemas. Application handlers stage UserManagementService repository changes and commit through `IDataChanges` once per command before publishing RabbitMQ events. Do not commit inside individual repositories.

Aspire and Compose opt in to startup migrations through `Database__ApplyMigrationsOnStartup=true`. Outside local orchestration apply migrations explicitly. See [docs/usermanagement-postgresql.md](docs/usermanagement-postgresql.md) for commands.

UserManagementService starts with empty PostgreSQL tables; no Mongo business data, Hangfire history or pending jobs are imported. Existing Mongo volumes, ScheduleService records and Keycloak data must not be deleted automatically. Existing schedules may retain references to old users/departments. Keep string IDs compatible with the 24-character hex API format.
