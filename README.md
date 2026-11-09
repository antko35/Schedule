# Schedule

Employee-schedule management system built with .NET 10 microservices. .NET Aspire orchestrates local development with Keycloak, RabbitMQ, and PostgreSQL.

## Repository map

| Path | Purpose |
| --- | --- |
| `src/AppHost/` | Aspire entry point for local orchestration. |
| `src/ServiceDefaults/` | Shared observability, health-check, service-discovery, and resilience defaults. |
| `src/Services/` | ScheduleService and UserManagementService. |
| `tests/` | Unit and application tests. |
| `../scheduleFe/` | Active Angular frontend; a separately managed Git repository. |
| `../front/electron-vue/` | Archived Electron/Vue frontend; a separately managed Git repository. |
| `docs/reference-materials/` | Historical spreadsheets and planning notes; not runtime assets. |
| `docker-compose.yml` | Docker Compose alternative for local infrastructure and services. |

The backend runtime template is embedded at `src/Services/ScheduleService/Application/Resources/scheduleTemplate.xlsx`; do not use the reference-material files as application resources.

## Run locally

Prerequisites: .NET SDK 10, Docker Desktop, and the user secrets required by the AppHost.

```powershell
dotnet run --project src/AppHost
```

For the Docker Compose alternative:

```powershell
docker-compose up --build
```

## Build and test

```powershell
dotnet build Schedule.sln
dotnet test Schedule.sln
```

## Core workflow

1. Create a user through UserManagementService.
2. Create a department and add the user to it.
3. The membership event creates a monthly schedule and generation rule in ScheduleService.
4. Configure the rule, then create schedules manually or let Hangfire generate them.

For architecture, ports, API details, and contributor guidance, see [AGENTS.md](AGENTS.md).

## PostgreSQL

Both services use `application_db`, with separate business schemas (`schedule`, `user_management`), EF migration histories and Hangfire schemas. No MongoDB server is required. UserManagementService begins empty; existing data and jobs are not imported. Existing database volumes and ScheduleService/Keycloak data are preserved.

See [UserManagementService PostgreSQL setup](docs/usermanagement-postgresql.md) for migration, restart, recovery, ID compatibility and old-reference consequences. Integration tests require Docker; see [UserManagementService tests](tests/UserManagementTests/PostgreSqlIntegration/README.md).
