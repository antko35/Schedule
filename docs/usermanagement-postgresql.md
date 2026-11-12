# UserManagementService PostgreSQL

Both services use the `applicationDb` connection (`ConnectionStrings__applicationDb` in environment variables). Local Compose exposes `application_db` on port 5433. UserManagementService owns `user_management` and `user_management_hangfire`; ScheduleService retains `schedule` and `schedule_hangfire`. Each EF context has its own migration history. No cross-service foreign keys are created.

## Start and restart

From `back/` with .NET 10 and Docker:

```bash
dotnet run --project src/AppHost
```

Or (Compose starts Keycloak in development mode and imports the same realm as Aspire):

```bash
docker compose up --build
```

If the host can reach NuGet but Docker's bridge network cannot, build on Linux with host networking, then reuse that image:

```bash
docker build --network host -t back-user-management-service -f src/Services/UserManagementService/UserManagementService.API/Dockerfile .
docker compose up --no-build
```

The image tag above assumes the default Compose project name `back`; adjust it if using a custom project name. Build the ScheduleService image separately if it is not already present.

Aspire and Compose explicitly set `Database__ApplyMigrationsOnStartup=true`. Migrations run before requests or workers start. Restarting preserves data and updates the single daily `update-ages-daily` definition. Hangfire schedules it at 00:00 UTC; age calculation retains the current server-local date rule. Hangfire prepares only its dedicated schema.

Outside local orchestration, provide a PostgreSQL connection and apply business migrations explicitly before starting with startup migration disabled:

```bash
export ConnectionStrings__applicationDb='Host=localhost;Port=5433;Database=application_db;Username=postgres;Password=postgres'
dotnet ef database update --project src/Services/UserManagementService/Data
dotnet run --project src/Services/UserManagementService/UserManagementService.API
```

The EF command requires an installed EF Core 9 tool. Direct API startup also requires RabbitMQ and Keycloak service-discovery settings; Aspire or Compose supplies them automatically. Schema setup needs PostgreSQL permissions to create the service-owned schemas and tables. Missing connections or failed migrations stop startup; there is no Mongo fallback.

## Transition

Stop the old UserManagementService and its Hangfire worker before switching configuration. The new service starts with empty business tables; no old users, departments, clinics, memberships, job history or pending jobs are read or imported. Recreate the required users and departments through the existing API. IDs remain 24-character hex strings generated without Mongo packages; HTTP routes, JSON and RabbitMQ message fields are unchanged.

Existing ScheduleService and Keycloak data remain intact. Old schedules may reference users and departments that no longer exist. The transition does not match records by name, remap IDs, or delete those schedules. Handling such old schedules is a separate data decision.

Mongo/Mongo Express and the unused `user-service-pg-db` container are no longer provisioned. Their existing volumes are left on disk. Do not use `docker compose down -v`, delete old volumes, or clear ScheduleService/Keycloak as part of this change. pgAdmin can connect to `application-db:5432`, database `application_db`, with the configured PostgreSQL credentials.

## Failure and recovery

If schema setup fails, keep the service stopped, correct credentials/permissions or the migration error, and retry. Do not delete a database to bypass an error. An already applied migration is not reapplied. A failed local command rolls back its database writes; successful commits precede RabbitMQ publication.

If RabbitMQ publishing fails after commit, the HTTP operation reports an error and diagnostic logs record it, but the committed SQL data remains. Delivery is not guaranteed across that gap; there is no transactional outbox in this change. Inspect the saved state before retrying an operation.

Returning to the previous application version requires its previous Mongo configuration and retained volumes. PostgreSQL writes are not copied back, so this is not a lossless data rollback. No automatic rollback or volume deletion is performed.

## Verification

```bash
dotnet build Schedule.sln
dotnet test Schedule.sln
docker compose config --quiet
```

Tests use disposable PostgreSQL/RabbitMQ containers and do not touch existing volumes. The integration suite includes both APIs and real membership events/RPC, with test authentication in place of live Keycloak tokens. See the test README for coverage and limitations.
