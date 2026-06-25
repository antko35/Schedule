# Frontend Core

Angular 19 frontend for medical staff schedule management. Lives in `FrontendAngular/` (own `.git`). **Not yet scaffolded** — CLAUDE.md defines the intended structure.

## Pages
- `/schedule` — monthly schedule view/edit per doctor
- `/departments` — department list, staff accordion + search
- `/control` — control panel with sidebar (color-coded CRUD actions + forms)

## Intended source layout
```
src/app/
  core/
    auth/           # Keycloak wrapper, AuthGuard
    interceptors/   # AuthInterceptor (Bearer), ErrorInterceptor
    models/         # Global shared interfaces
  shared/
    components/     # AvatarComponent, ToastComponent, …
    pipes/          # initials, shiftLabel, …
    utils/          # pure functions: sort, date, form
  features/
    schedule/       { components, services, store, models, schedule.routes.ts }
    departments/    { components, services, store, models, departments.routes.ts }
    control-panel/  { components, services, store, models, control-panel.routes.ts }
styles/
  tokens.css        # all CSS custom props
  global.css
environments/
  environment.ts
  environment.prod.ts
```

## API docs (source of truth)
`docs/schedule-service.swagger.json` — port 5002  
`docs/usermanagement-service.swagger.json` — port 5003  
`user-service.swagger.json` expected at port 5001 (not yet present in docs/)

## Design system
`claudeDesign/` — design tokens, preview HTML files, adherence lint rules  
`claudeDesign/colors_and_type.css` — canonical CSS vars

## Feature memories
- Per-feature implementation notes: `mem:frontend/schedule-feature`, `mem:frontend/departments-feature`, `mem:frontend/control-panel-feature` (write when features are built)
- Auth details: `mem:frontend/auth-setup`
- Shared components: `mem:frontend/shared-components`
