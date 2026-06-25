# Frontend Task Completion

Run from `FrontendAngular/` after every coding task:

```powershell
ng lint                        # ESLint — must pass clean
npx tsc --noEmit               # type-check without emitting
ng test --watch=false          # Jest — all tests must pass
```

Then:
- Update the relevant `mem:frontend/*-feature` Serena memory
- Update the corresponding Obsidian doc in `docs/obsidian/` (English, concise)

## Obsidian doc structure
```
docs/obsidian/
  pages/      schedule.md, departments.md, control-panel.md
  shared/     components.md, pipes.md
  architecture/ overview.md, state-management.md, auth.md
  api/        contracts.md
```
