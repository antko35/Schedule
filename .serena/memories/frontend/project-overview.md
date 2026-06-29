## Angular Frontend — Project Overview

### Stack
- Angular 19, standalone components everywhere
- NgRx Signal Store (`@ngrx/signals@19`)
- PrimeNG 19 + `@primeng/themes@19`
- keycloak-angular@19 + keycloak-js
- CSS custom properties (no SCSS)
- HTTP via functional interceptors

### Key files
- `src/environments/environment.ts` — service URLs + Keycloak placeholder
- `src/app/core/models/environment.token.ts` — `ENVIRONMENT` InjectionToken (inject in services)
- `src/styles/tokens.css` — all CSS custom properties
- `src/styles/global.css` — reset, scrollbar, body
- `src/app/app.config.ts` — providers: router, HttpClient + interceptors, KeycloakService
- `src/app/app.routes.ts` — routes with lazy loading and authGuard

### Folder structure
```
src/app/
  core/auth/           → authGuard
  core/interceptors/   → authInterceptor, errorInterceptor
  core/models/         → ENVIRONMENT token, global interfaces
  shared/components/   → NavPillComponent
  shared/pipes/
  shared/utils/
  features/schedule/components/services/store/models/
  features/departments/components/services/store/models/
  features/control-panel/components/services/store/models/
```

### Routing
- `/` → redirect to `/schedule`
- `/schedule` → SchedulePageComponent (lazy)
- `/departments` → DepartmentsPageComponent (lazy)
- `/control` → ControlPanelPageComponent (lazy)
- All routes guarded by authGuard

### Status
- Session 1 complete: scaffold, routing, tokens.css, interceptors, NavPill
- Session 2 complete: Departments page fully implemented (store, service, 3 components, tests)
  See `mem:frontend/departments-feature` for full details.
- Sessions 3–8: pending (control panel, schedule page, tests polish)
