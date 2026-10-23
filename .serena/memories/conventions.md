---
name: conventions
description: Code style, naming, and design patterns specific to this codebase
metadata:
  type: project
---

## C# conventions
- StyleCop enforced (SA* warnings). New files should put `using` directives inside namespace declarations.
- MediatR: every use case is a `record` command/query + a handler class implementing `IRequestHandler<TReq>` or `IRequestHandler<TReq, TResp>`.
- Repository pattern: `IGenericRepository<T>` base interface with `AddAsync`, `GetByIdAsync`, `UpdateAsync`, `GetAllAsync`. Service-specific repos extend it.
- `EnsureExists(message)` extension on nullable entities to throw if null.
- Hangfire jobs registered in `Hangfire.cs` (`ConfigureHangfire`) via `IRecurringJobManager.AddOrUpdate`. Jobs dispatch MediatR commands.
- MongoDB entities inherit from `Entity` base class with `[BsonElement]` attributes.
- `ScheduleRulesGenerator` is a service (not static) injected where needed. It creates blank `UserScheduleRules` + `Schedule` for a user/department/month.
- `ScheduleGenerator` is a **static** class with `GenerateWorkDaysForUser`.

## Naming
- Command handlers: `<Action>CommandHandler : IRequestHandler<Command>`
- Query handlers: `<Action>QueryHandler : IRequestHandler<Query, Result>`
- Repositories: `I<Entity>Repository` interface + `<Entity>Repository` implementation
- Month names stored as lowercase English strings (e.g. `"january"`)

## Frontend (Electron + Vue 3 TypeScript)
- All API calls go through `window.storageAPI` (Electron IPC preload)
- IPC channel names: `storage:<action>` pattern
- Cache keys: descriptive strings like `scheduleRules`, `schedules_{userId}_{deptId}_{year}_{month}`
- Cache invalidation via `window.storageAPI.setCacheValidity(key, false)`
