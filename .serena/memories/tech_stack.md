---
name: tech_stack
description: Language, frameworks, build tools, and key packages for the Schedule solution
metadata:
  type: project
---

- **Language**: C# / .NET 10 (projects target net10.0; some bin artifacts show net9.0 for older builds)
- **Frontend**: Electron + Vue 3 (TypeScript) in `Front/electron-vue/`
- **Orchestration**: .NET Aspire (`Schedule.AppHost`)
- **Auth**: Keycloak (JWT, realm `schedule-service`, audience `schedule-service`)
- **Messaging**: RabbitMQ (Aspire.RabbitMQ.Client)
- **gRPC**: inter-service calls (UserService ↔ UserManagementService)
- **Background jobs**: Hangfire with Mongo storage (`HangfireDb`)
- **ScheduleService DB**: MongoDB (Aspire.MongoDB.Driver)
- **UserService DB**: PostgreSQL (Entity Framework Core)
- **Validation**: FluentValidation with MediatR pipeline behavior
- **Mapping**: AutoMapper
- **Linting**: StyleCop analyzers (warnings only, not errors)
- **Testing**: xUnit, Moq, FluentAssertions
