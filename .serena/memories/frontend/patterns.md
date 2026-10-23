## Patterns

### ENVIRONMENT injection token
`core/models/environment.token.ts` exports `ENVIRONMENT` InjectionToken.
Provided in root via factory: `factory: () => environment`.
Services inject it: `private readonly env = inject(ENVIRONMENT)`.
**Never hardcode URLs** — always use `env.userServiceUrl` etc.

### AuthInterceptor token handling
Uses `from(keycloak.getToken().catch(() => null))` to handle uninitialized Keycloak.
If token is null/empty → passes request through unchanged.
If token present → clones request with `Authorization: Bearer <token>`.

### AuthGuard dev bypass
Wraps `keycloak.isLoggedIn()` in try/catch.
If throws (Keycloak not initialized) → returns `true`.
If returns false → calls `keycloak.login()` then returns false.
This makes the app work without a Keycloak server during development.

### NavPill border color (active route)
Uses `toSignal(router.events.pipe(filter NavigationEnd, map urlAfterRedirects, startWith router.url))`.
`borderColor` is a `computed()` that maps URL prefix to CSS variable.
Applied via `[style.border-bottom-color]="borderColor()"` on the nav element.

### Form reset after store success
Smart page uses `viewChild(FormComponent)` (one per form type) and an `effect()` that
watches `store.successMessage()`. On truthy value: calls `messageService.add(success)`,
then calls `.reset()` on every viewChild ref (only the visible one is non-null), then
`store.clearToast()`. Errors use a separate effect watching `store.error()`.

### Lazy data loading triggered by activeForm
Forms needing remote data (user list, dept list) are not pre-loaded.
The smart page has an `effect()` watching `store.activeForm()` and calls
`store.loadUsers()` / `store.loadDepartments()` only when the relevant form becomes active.

### Lazy loading
All feature routes use `loadComponent` with dynamic import.
Pattern: `loadComponent: () => import('./features/...').then(m => m.ComponentClass)`
