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
Provided in `app.config.ts` with an `APP_INITIALIZER` that calls `keycloak.init()`.
Uses `onLoad: 'login-required'` — app redirects to Keycloak login immediately on load if not authenticated.
The `.catch(() => false)` lets the app continue if Keycloak is unreachable (dev without Aspire).

### Keycloak config location
`src/environments/environment.ts` → `keycloak.url = 'http://localhost:8080'`, `realm = 'schedule-service'`, `clientId = 'schedule-frontend'`.
These are real values matching the backend realm and Keycloak setup.

### Runtime config (Aspire dynamic Keycloak port)
`proxy.conf.js` writes `public/runtime-config.json` at dev-server start with `keycloakUrl` from
`process.env['services__keycloak__http__0']`. Falls back to `environment.keycloak.url`.
Angular's `APP_INITIALIZER` fetches `/runtime-config.json` first, then passes the URL to `keycloak.init()`.
`public/runtime-config.json` is gitignored (generated file).

### AppHost notes
- `AddKeycloak` must have `.WithExternalHttpEndpoints()` so the container port is published to localhost
  and the browser can reach it for the login redirect.
- `services__keycloak__http__0` is injected into the Angular Node process via `.WithReference(keycloak)`.

### onLoad strategy
Use `check-sso` (NOT `login-required`).
`login-required` causes an infinite reload loop when the code exchange fails — it redirects to login
again immediately instead of resolving. `check-sso` resolves `false` → guard does one controlled redirect.

### AuthGuard loop prevention
Guard checks for `code` + `state` in the URL before calling `keycloak.login()`.
If both are present, the APP_INITIALIZER is mid-exchange — guard returns `false` without starting
a second redirect.

### Status (Session 3)
Fully wired: APP_INITIALIZER → fetch runtime-config.json → keycloak.init (check-sso, dynamic URL)
→ guard redirects to login once if needed → interceptor attaches token → backend accepts requests.
