# UserManagementService: SaveChanges pattern and review

Reviewed on 2026-11-09 against the uncommitted backend changes, including new files.
The active `scheduleFe` working tree was clean. This review did not change application code.

## Review result

No confirmed correctness defects were found in the reviewed changes. The migration
keeps persistence in DataAccess and command boundaries in Application. EF Core and
Hangfire behavior was checked using Context7 CLI because the configured Context7
MCP tools were not exposed in the review session.

Validation from `back/`:

- `dotnet test tests/UserManagementTests/ClassLibrary3/UserManagementService.csproj --no-restore --verbosity quiet`: 74 passed, 0 failed, 0 skipped.
- `dotnet test tests/UserManagementTests/PostgreSqlIntegration/PostgreSqlIntegration.csproj --no-restore --verbosity quiet`: 11 passed, 0 failed, 0 skipped.
- `git diff --check`: passed.
- `docker compose config --quiet`: passed.

Integration coverage includes PostgreSQL migrations and restart persistence,
constraints, rollback, HTTP behavior, canonical IDs, commit/publication behavior,
Hangfire definition persistence, and real cross-service RabbitMQ events and RPC.
The tests substitute authentication and do not exercise live Keycloak login or
execution by a real Hangfire worker. A full solution build/test and container image
build were not repeated during this review. Build output includes existing package
vulnerability and analyzer warnings; passing tests do not resolve those warnings.

## Pattern to preserve

1. Register `UserManagementDbContext` with a scoped lifetime. Resolve `IDataChanges`
   to that same context instance, so every repository participating in a command
   shares its change tracker and commit boundary.
2. Repositories stage inserts, updates and deletes. They do not call
   `SaveChangesAsync`, publish messages, or open independent commit boundaries.
   Awaiting `AddAsync`, `UpdateAsync`, or `RemoveAsync` does not mean data is durable.
3. The application handler validates business conditions and stages every local
   change, then calls `await changes.SaveChangesAsync(cancellationToken)` once.
   Keep the inherited EF implementation; no custom exception translation is part
   of this pattern.
4. Update an entity loaded through the current context rather than constructing a
   second entity with the same key. Current handlers call `UpdateAsync` on that
   instance. Its `DbSet.Update` implementation marks the whole entity modified;
   it is not a property-level partial update or optimistic concurrency mechanism.
5. Before deleting a user or department, materialize the memberships needed for
   events, stage deletion of memberships and their parent, and commit together.
   Restrict foreign keys protect against orphan memberships. Database unique
   constraints supplement application duplicate checks under concurrent requests.
6. Publish RabbitMQ events only after a successful commit, using canonical IDs
   from persisted entities or the membership snapshot. A failed commit must not
   publish an event.

Example of the existing command boundary:

```csharp
await userJobsRepository.AddAsync(newUserJob);
await changes.SaveChangesAsync(cancellationToken);
await userEventPublisher.PublishUserCreated(newUserJob.UserId, newUserJob.DepartmentId);
```

A single relational `SaveChangesAsync` atomically commits its database writes.
It does not make preceding reads and subsequent RabbitMQ operations one atomic
operation. If publication fails, SQL remains committed and the request can return
an error. There is no outbox or guaranteed eventual delivery in this change;
inspect saved state before retrying. See [transition and recovery](usermanagement-postgresql.md).

## Storage and job details

- Business tables and EF migration history belong to `user_management`;
  Hangfire storage belongs to `user_management_hangfire`. Keep both separate from
  ScheduleService schemas.
- Runtime migrations are opt-in through `Database:ApplyMigrationsOnStartup` and
  run before host startup. Design-time tooling uses the same history schema.
- IDs remain lowercase 24-character hexadecimal strings, generated without Mongo
  dependencies. Normalize incoming ID comparisons and publish canonical IDs.
- Map `DateOfBirth` through its backing field so EF materialization does not invoke
  the age-calculating setter. `Age` is persisted and explicitly updated by commands
  and the daily job.
- Hangfire schedules a typed `UpdateUserAgesJob`; its mediator and repositories
  must be resolved within the job scope. Preserve the single `update-ages-daily`
  definition at 00:00 UTC and the application's existing local-date age rule.

## Documentation checked through Context7

The project uses EF Core/Npgsql 9.0.2, Hangfire.AspNetCore 1.8.17 and
Hangfire.PostgreSql 1.21.1. Context7 returned current upstream documentation rather
than documentation pinned to each of these package versions; the integration
tests above exercise the project's installed packages.

- [EF Core saving and change tracking](https://github.com/dotnet/entityframework.docs/blob/main/entity-framework/core/saving/index.md)
- [EF Core transactions](https://github.com/dotnet/entityframework.docs/blob/main/entity-framework/core/saving/transactions.md)
- [EF Core explicit tracking and Update](https://github.com/dotnet/entityframework.docs/blob/main/entity-framework/core/change-tracking/explicit-tracking.md)
- [EF Core property and field access](https://github.com/dotnet/entityframework.docs/blob/main/entity-framework/core/change-tracking/miscellaneous.md)
- [Hangfire dependency injection](https://github.com/hangfireio/hangfire.documentation/blob/main/background-methods/using-ioc-containers.rst)
- [Hangfire 1.8 recurring-job options](https://github.com/hangfireio/hangfire.documentation/blob/main/upgrade-guides/upgrading-to-hangfire-1.8.rst)
