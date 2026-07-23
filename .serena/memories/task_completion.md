---
name: task_completion
description: Commands to verify a coding task is done
metadata:
  type: project
---

After any backend change:
```
dotnet build src/ScheduleService/ScheduleService.API/ScheduleService.API.csproj --no-restore -v quiet
```
(or the relevant service's API project — it pulls all dependencies)

Check for **0 Error(s)** in output. StyleCop SA* warnings are pre-existing and acceptable.

For UserService changes:
```
dotnet build src/UserService/UserService.API/UserService.API.csproj --no-restore -v quiet
```

For UserManagementService changes:
```
dotnet build src/UserManagementService/UserManagementService.API/UserManagementService.API.csproj --no-restore -v quiet
```

Run tests:
```
dotnet test Schedule.sln
```

Frontend changes (Electron + Vue): manually run the app via `npm run dev` inside `legacy/electron-vue/electron-vue/` and verify the UI.
