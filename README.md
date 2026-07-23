# Schedule

Employee-schedule management system built with .NET 10 microservices. .NET Aspire orchestrates local development with Keycloak, RabbitMQ, PostgreSQL, and MongoDB.

## Repository map

| Path | Purpose |
| --- | --- |
| `src/AppHost/` | Aspire entry point for local orchestration. |
| `src/ServiceDefaults/` | Shared observability, health-check, service-discovery, and resilience defaults. |
| `src/Services/` | UserService, ScheduleService, and UserManagementService. |
| `tests/` | Unit and application tests. |
| `apps/web/` | Active Angular frontend; a separately managed Git repository. |
| `legacy/electron-vue/` | Archived Electron/Vue frontend; a separately managed Git repository. |
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
3. The membership event creates a `UserScheduleRule` in ScheduleService.
4. Configure the rule, then create schedules manually or let Hangfire generate them.

For architecture, ports, API details, and contributor guidance, see [AGENTS.md](AGENTS.md).
