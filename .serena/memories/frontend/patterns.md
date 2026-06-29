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

### Lazy loading
All feature routes use `loadComponent` with dynamic import.
Pattern: `loadComponent: () => import('./features/...').then(m => m.ComponentClass)`
