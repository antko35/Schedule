---
name: suggested_commands
description: Commands to run the solution, build, test, and run migrations
metadata:
  type: project
---

# Run (preferred — Aspire orchestrates everything)
```
dotnet run --project src/AppHost
```

# Alternative
```
docker-compose up --build
```

# Build
```
dotnet build Schedule.sln
```

# Test all
```
dotnet test Schedule.sln
```

# Test individual
```
dotnet test tests/UserServiceTests/UserServiceTests.csproj
dotnet test tests/ScheduleServiceTests/Application/Application.csproj
dotnet test tests/UserManagementTests/ClassLibrary3/UserManagementService.csproj
```

# EF Core migrations (UserService only)
```
dotnet ef migrations add <Name> -s src/UserService/UserService.API -p src/UserService/Data
dotnet ef database update -s src/UserService/UserService.API -p src/UserService/Data
```

# Build single service (faster check)
```
dotnet build src/ScheduleService/ScheduleService.API/ScheduleService.API.csproj --no-restore
```
