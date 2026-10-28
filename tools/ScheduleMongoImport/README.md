# Schedule MongoDB to PostgreSQL import

This is a one-time importer for the legacy `schedule_db` collections `UserScheduleRule`, `Schedule`, and `Calendar`. It does not change MongoDB. It preserves the Mongo `_id` values as PostgreSQL string IDs and creates deterministic IDs for embedded work shifts.

## Before running

1. Back up MongoDB and PostgreSQL. Stop writes to ScheduleService for the entire dry run, import, and verification window.
2. Apply the `InitialScheduleSchema` EF migration to the target PostgreSQL database. The importer checks for pending migrations and does not create schema.
3. Set these environment variables in the same shell:

   - `SCHEDULE_IMPORT_MONGO_URI`: connection string for the existing MongoDB server.
   - `SCHEDULE_IMPORT_MONGO_DATABASE`: legacy database name, normally `schedule_db`.
   - `SCHEDULE_IMPORT_POSTGRES`: connection string for the new `application_db`.
   - `SCHEDULE_IMPORT_TIME_ZONE`: IANA/Windows time zone for work dates, for example `Europe/Warsaw`. This must match the clinic's historical scheduling time zone.

4. Run `dotnet run --project tools/ScheduleMongoImport/ScheduleMongoImport.csproj` for the default dry run. Inspect the reported counts and correct any rejected source records before proceeding.
5. Run the same command with `-- --apply` to insert records. Run the default dry run again afterward. A repeated `--apply` skips matching records and rejects conflicting records.

The importer loads the three collections and validates ownership, duplicate periods and dates, rule flags, exact whole-minute durations, embedded shift intervals, and local work dates before writing. It rejects schedules without an owning rule, missing schedule references, and target rows with the same ID but different data. A single PostgreSQL transaction wraps the inserts.

The old `startWorkDayTime` becomes `first_shift_start`; `second_shift_start` uses the former generator constant `14:30`. BSON shift timestamps are preserved as UTC instants. The source has no creation/update timestamps, so imported schedules receive the import time. The calendar's historical empty transfer date becomes `NULL`.

The tool intentionally stops on unsupported BSON date/time representations or ambiguous data. It does not silently repair source records. Validate a copy of production data before the cutover, then compare source and destination counts and representative API responses. Keep the Mongo backup for rollback.

## Verification

The regression test in `tests/ScheduleServiceTests/PostgreSqlIntegration/MongoImportTests.cs` exercises actual legacy-driver BSON date/time representations, including `TimeOnly` ticks and `DateTimeOffset` arrays/documents. Run the integration project with Docker to verify dry run, repeated apply, preserved IDs, and conflict rejection against disposable MongoDB/PostgreSQL instances.
