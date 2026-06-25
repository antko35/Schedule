# Keycloak Authentication Guide

## What is Keycloak?

Keycloak is an open-source Identity and Access Management server. In this project it acts as the **single source of truth for who is allowed to call the APIs**. Instead of building login/logout/password logic yourself, your services delegate that entirely to Keycloak.

Core concepts you need to know:

| Term | What it means here |
|---|---|
| **Realm** | An isolated authentication space. This project uses `schedule-service`. Think of it as "the tenant". |
| **Client** | An application registered inside the realm. This project has one client: `schedule-service`, used by both backend APIs and Swagger UI. |
| **User** | A person with a username/password who can log in. |
| **Token** | A signed JWT the user receives after login. APIs verify this token on every request. |
| **Audience (`aud`)** | A claim inside the token that says which service the token is meant for. Both ScheduleService and UserManagementService require `aud: schedule-service`. |

---

## How it works in this project

```
Browser / Swagger UI
        │
        │  1. Login (username + password)
        ▼
   Keycloak :8080
        │
        │  2. Returns JWT access token
        ▼
  Your API Request
  Authorization: Bearer <token>
        │
        │  3. Service validates token signature + audience
        ▼
  ScheduleService / UserManagementService
```

The two services that require authentication:
- **ScheduleService** (port 5002) — all endpoints
- **UserManagementService** (port 5003) — all endpoints

**UserService** (port 5001) is an internal gRPC service and does not use Keycloak auth.

---

## First-time setup (automatic via Aspire)

When you run the project via Aspire (`dotnet run --project Schedule.AppHost`), Keycloak starts automatically and the realm is imported from `Schedule.AppHost/keycloak/schedule-service-realm.json`.

**This creates:**
- Realm `schedule-service`
- Client `schedule-service` (public, PKCE-enabled)
- One built-in user: `admin` / `Admin1234!`

> The realm import runs only once on a fresh Keycloak data volume. If Keycloak already has the realm (from a previous run with `.WithDataVolume()`), it will not re-import.

---

## Logging in via Swagger UI

Both services expose Swagger UI at their root URL (e.g., `http://localhost:{port}/`).

**Steps:**

1. Open Swagger UI for the service you want to test.
2. Click the **Authorize** button (padlock icon, top right).
3. A dialog opens. Leave `client_id` as `schedule-service`. Click **Authorize**.
4. Keycloak's login page opens in a popup. Log in with:
   - **Username:** `admin`
   - **Password:** `Admin1234!`
5. After login, the popup closes. Click **Close** in Swagger.
6. All API calls from Swagger now include the Bearer token automatically.

To log out, click **Authorize** → **Logout**.

---

## Calling the API directly (e.g., from Postman or curl)

### Step 1: Get a token

Find the Keycloak port in the Aspire dashboard (it changes each run unless you fix it).

```bash
curl -X POST http://localhost:{KEYCLOAK_PORT}/realms/schedule-service/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=schedule-service" \
  -d "username=admin" \
  -d "password=Admin1234!"
```

Response:
```json
{
  "access_token": "eyJhbGci...",
  "expires_in": 3600,
  ...
}
```

### Step 2: Use the token

```bash
curl http://localhost:5002/Calendar/holidays/2025 \
  -H "Authorization: Bearer eyJhbGci..."
```

### Postman setup

1. In your collection, go to **Authorization** tab.
2. Type: **OAuth 2.0**
3. Grant Type: **Authorization Code (With PKCE)**
4. Auth URL: `http://localhost:{KEYCLOAK_PORT}/realms/schedule-service/protocol/openid-connect/auth`
5. Access Token URL: `http://localhost:{KEYCLOAK_PORT}/realms/schedule-service/protocol/openid-connect/token`
6. Client ID: `schedule-service`
7. Scope: `openid profile`
8. Click **Get New Access Token**, log in, then **Use Token**.

---

## Adding more users

Keycloak has an admin console at `http://localhost:{KEYCLOAK_PORT}`. Default admin credentials: `admin` / `admin` (these are the Keycloak admin account, separate from the `admin` user in the realm).

**To add a user:**

1. Open `http://localhost:{KEYCLOAK_PORT}` → log in as `admin` / `admin`.
2. Select realm **schedule-service** from the top-left dropdown.
3. Go to **Users** → **Create new user**.
4. Fill in Username, Email, First/Last name. Toggle **Email verified** to ON.
5. Click **Create**.
6. Go to the **Credentials** tab → **Set password** → enter a password → turn off **Temporary** → **Save**.

The new user can now log in and call the APIs.

---

## How user attributes (claims) work

UserManagementService exposes `ClaimsController` (`/AddClaim`, `/DeleteClaim`, `/GetClaims/{email}`) and `PermissionsController` (same endpoints, at `/Permissions/...`).

These endpoints write to Keycloak's user **attributes** — arbitrary key-value metadata stored on each user. The attribute values end up in the JWT token if you configure a protocol mapper for them (see "What else you can configure" below).

**Example: add a custom attribute**

```json
POST /AddClaim
{
  "email": "admin@schedule.local",
  "claimType": "department",
  "claimValue": "radiology"
}
```

This calls the Keycloak Admin API and sets `attributes.department = ["radiology"]` on the user.

---

## What else you can configure

These are the common next steps when you want to grow the auth system:

### 1. Map user attributes into JWT tokens

Right now, custom attributes (set via `AddClaim`) are stored in Keycloak but **not** automatically included in tokens. To include them:

1. Keycloak admin console → Realm `schedule-service` → **Clients** → `schedule-service` → **Client scopes** tab.
2. Click the linked scope → **Mappers** → **Add mapper** → **By configuration** → **User Attribute**.
3. Set **User Attribute** = the attribute name (e.g., `department`), **Token Claim Name** = same value, toggle **Add to access token** = ON.

Your API can then read this claim from `User.FindFirst("department")?.Value`.

### 2. Add roles

1. Keycloak admin → Realm → **Realm roles** → **Create role** (e.g., `admin`, `viewer`).
2. Assign the role to a user: **Users** → select user → **Role mapping** tab → assign role.
3. The role appears in tokens under `realm_access.roles`.
4. In your API, enforce it with `[Authorize(Roles = "admin")]`.

### 3. Restrict self-registration

Self-registration is already **disabled** in the imported realm (`registrationAllowed: false`). Users can only be created by a Keycloak admin.

### 4. Use a confidential client (production hardening)

The current `schedule-service` client is **public** (no client secret). This is fine for SPAs and Swagger UI with PKCE.

If you add a backend service that calls the API on its own behalf (client credentials flow), create a separate **confidential** client with a secret, and use `grant_type=client_credentials`.

### 5. Configure token lifetimes

Default access token lifetime is **1 hour** (set in the realm JSON). To change it:

- Keycloak admin → Realm `schedule-service` → **Realm settings** → **Tokens** tab → **Access Token Lifespan**.

### 6. Enable HTTPS for Keycloak (production)

The realm is set to `"sslRequired": "external"` — meaning HTTPS is required for external connections but not for localhost. In production, configure a reverse proxy (nginx/Traefik) in front of Keycloak and set `sslRequired: all`.

### 7. Persist users across restarts without a data volume

The realm JSON in `Schedule.AppHost/keycloak/schedule-service-realm.json` is the source of truth for initial setup. Any users you create via the admin console are stored in Keycloak's database (the data volume). If you remove `.WithDataVolume()` from AppHost, those users are lost on restart. Keep the volume, or export the realm (including users) and commit it.

---

## Architecture reference

- Realm JSON: `Schedule.AppHost/keycloak/schedule-service-realm.json`
- AppHost wiring: `Schedule.AppHost/AppHost.cs` — `WithRealmImport("keycloak")`
- ScheduleService auth: `src/ScheduleService/ScheduleService.API/Program.cs` — `AddKeycloakJwtBearer`
- UserManagementService auth: `src/UserManagementService/UserManagementService.API/Program.cs` — `AddKeycloakJwtBearer`
- Keycloak Admin calls: `src/UserManagementService/UserManagementService.API/Keycloak/KeycloakAdminService.cs`
