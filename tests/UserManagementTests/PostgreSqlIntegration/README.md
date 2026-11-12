# UserManagementService PostgreSQL integration tests

From `back/`:

```bash
dotnet test tests/UserManagementTests/PostgreSqlIntegration/PostgreSqlIntegration.csproj
```

Requires Docker. Tests provision disposable PostgreSQL 17 containers; no existing databases or volumes are used. They check real EF migrations, persistence, dates and saved age, repository projections, duplicate constraints, foreign keys, atomic rollback and isolation from other schemas. API tests use the real pipeline and PostgreSQL with substituted authentication and event publisher; hosted workers are disabled in those API tests. A separate cross-service test starts real RabbitMQ and both API hosts, enables the actual membership consumers and email RPC server, and restarts both hosts while retaining PostgreSQL. ScheduleService runs in the ScheduleConsumerHost helper process so each service keeps its actual MediatR version. Live Keycloak tokens and scheduled Hangfire worker execution are outside this test harness; the daily job is executed directly through its normal scoped dependencies.

UserManagementService uses `user_management` and `user_management_hangfire`; ScheduleService uses separate schemas. The daily `update-ages-daily` definition is recreated at 00:00 UTC, and age calculation retains the application's local-date rule. Old Mongo data, pending jobs and job history are not imported.

Database commits precede RabbitMQ publishing. A broker failure after commit is logged and returned as an error, but does not undo SQL changes or guarantee eventual delivery. A transactional outbox is outside this change.
