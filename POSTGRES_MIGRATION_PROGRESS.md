> Historical ScheduleService migration record. Current storage for both services is PostgreSQL; see [UserManagementService transition](docs/usermanagement-postgresql.md). Older importer references below describe previous work and are not current run instructions.

# ScheduleService PostgreSQL migration progress

Last updated: 2026-10-28

## Scope and storage

- ScheduleService uses PostgreSQL database resource `applicationDb`, schema `schedule`, and migration history `schedule.__EFMigrationsHistory`.
- Hangfire uses schema `schedule_hangfire` in the same PostgreSQL database.
- UserManagementService remains on MongoDB. At the user’s request, the unused ScheduleService MongoDB and Mongo Express resources were removed from Aspire and Compose. Existing stored volumes were not deleted.
- The migration is integrated with main commit `89b6d5b`, including Aspire, Keycloak authentication, service discovery and the new source layout under `src/Services/ScheduleService`.
- Existing HTTP routes and DTO field names are preserved, including the newer PUT monthly rules, PATCH response, per-user generation, complete holiday list, bulk holiday deletion and workbook export.
- No live MongoDB data has been imported. The original shelf remains available as a backup.

## Implementation

- PostgreSQL domain entities, EF configurations, initial schema migration, repositories and a scoped EF Core DbContext replace ScheduleService Mongo storage. Handlers call `ScheduleDbContext.SaveChangesAsync` directly; no custom unit-of-work interface is used.
- Queries sharing a DbContext execute sequentially. Mutation handlers save their changes atomically.
- Manual entry creates a missing month and rules. Repeated rule saves reuse the existing month.
- Partial rule updates preserve omitted values, accept midnight and return the updated DTO.
- Aspire injects `ConnectionStrings__applicationDb`; ScheduleService no longer registers a Mongo client. Startup schema migration is enabled for the Aspire development resource.
- Compose preserves main's Keycloak configuration and adds PostgreSQL. The ScheduleService image uses .NET 10 and includes shared ServiceDefaults.
- Importer supports the legacy Mongo driver BSON representations of DateOnly, TimeOnly ticks and DateTimeOffset, preserves source IDs and UTC instants, rejects conflicting records, and wraps inserts in one transaction.

## Verification on 2026-10-28

- Full solution build passed, including Aspire AppHost.
- ScheduleService unit tests: 66 passed.
- PostgreSQL/API/import integration tests: 7 passed on disposable PostgreSQL 16/17 and MongoDB 7 containers.
- HTTP tests cover startup EF migration, Hangfire schema/job registration, monthly rule creation, partial updates, generation/regeneration, manual shift replacement, manual missing-month creation, export, cascading deletion, holiday DTOs and atomic bulk deletion.
- Import test verifies dry run writes nothing, first apply, repeated apply, preserved IDs and rejection of destination differences without overwriting them. Source Mongo documents remain intact.
- Aspire manifest generation passed: ScheduleService receives the PostgreSQL database connection, with no Mongo connection injected.
- Compose configuration validation and the .NET 10 ScheduleService container build passed.
- Full solution tests: 152 passed, 4 failed. All four failures are unchanged UserManagementService age tests with hard-coded ages from an earlier year. No UserManagementService source/tests were changed.
- Package-audit and existing analyzer warnings remain; this migration does not update unrelated dependency versions.

## Remaining before live cutover

1. Back up the real source and destination databases and stop ScheduleService writes during import and reconciliation.
2. Validate the importer dry run against a backed-up copy of the actual Mongo data. Resolve any rejected records explicitly.
3. Apply the import only after the backup and dry-run review, then compare counts and representative API responses against the original data.
4. Validate the complete Aspire flow with real Keycloak authentication and RabbitMQ delivery. HTTP integration tests substitute authentication and disable background consumers/job execution; they do not establish live external-service behavior.
5. Rollback is no longer required by the user. The old ScheduleService Mongo resources are no longer provisioned; any stored volumes remain untouched. Do not migrate UserManagementService implicitly.

The pre-existing `createRules/{departmentId}/{year}/{month}` endpoint was already a no-op on main and remains so. The implemented PUT monthly-rules endpoint creates the selected month.

## Commands

```powershell
dotnet build Schedule.sln
dotnet test tests/ScheduleServiceTests/Application/Application.csproj
dotnet test tests/ScheduleServiceTests/PostgreSqlIntegration/PostgreSqlIntegration.csproj
```

The integration project and importer are now included in the solution. Running `dotnet test Schedule.sln` requires Docker. The importer is dry-run-first; see `tools/ScheduleMongoImport/README.md` before using `--apply`.
