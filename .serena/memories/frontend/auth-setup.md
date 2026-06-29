## Auth Setup

### Files
- `core/auth/auth.guard.ts` — `CanActivateFn`
- `core/interceptors/auth.interceptor.ts` — functional `HttpInterceptorFn`
- `core/interceptors/error.interceptor.ts` — functional `HttpInterceptorFn`

### Dev bypass (important)
Both `authGuard` and `authInterceptor` wrap Keycloak calls in try/catch.
If Keycloak is not initialized (no server running), they fail gracefully:
- Guard: returns `true` (all routes pass through)
- Interceptor: passes request without Authorization header

### KeycloakService
Provided as a plain value in `app.config.ts` providers array.
NOT initialized via `APP_INITIALIZER` yet — add that when Keycloak server is ready.

### Keycloak config location
`src/environments/environment.ts` → `keycloak.url`, `.realm`, `.clientId`
Currently all set to placeholder values.

### Adding real Keycloak
1. Fill in environment.ts with real realm/clientId
2. Add APP_INITIALIZER to app.config.ts that calls `KeycloakService.init({ config: environment.keycloak })`
3. Guard and interceptor will automatically use the real session
