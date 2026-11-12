# ScheduleService PostgreSQL integration tests

Run with Docker available:

```powershell
dotnet test tests/ScheduleServiceTests/PostgreSqlIntegration/PostgreSqlIntegration.csproj
```

The project is included in `Schedule.sln`, so running all solution tests now requires Docker.
Tests create and remove disposable PostgreSQL 16/17 containers. They apply the real EF migration and verify schema, uniqueness constraints, tracked shift updates, HTTP contract round trips, regeneration, manual month creation, export, cascaded deletion, and atomic holiday deletion.

HTTP tests use test authentication and disable hosted background jobs/consumers. They do not validate live Keycloak authentication, RabbitMQ delivery, or email delivery. Hangfire's PostgreSQL schema and recurring-job registration run against the disposable database during API startup.

Existing local databases and volumes are not used.
