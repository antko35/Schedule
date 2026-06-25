# Building the Electron Desktop App

The frontend is an Electron desktop application ("Расписание"). It must be installed on every
machine that will use it. The backend services (APIs, MongoDB, RabbitMQ, Keycloak) run on a
server and are separate from the installer.

## Quick build (Windows installer)

```bash
cd Front/electron-vue
npm run build
```

Output: `Front/electron-vue/release/<version>/Расписание-Windows-<version>-Setup.exe`

## Changing backend server URLs

### Where the URLs are configured

File: `Front/electron-vue/electron/config/apiConfig.ts`

```typescript
export const API_CONFIG = {
    USER_SERVICE:    (process.env.VITE_USER_API_URL    ?? 'http://localhost:5003').replace(/\/$/, ''),
    SCHEDULE_SERVICE:(process.env.VITE_SCHEDULE_API_URL ?? 'http://localhost:5002').replace(/\/$/, ''),
    KEYCLOAK_URL:    (process.env.VITE_KEYCLOAK_URL    ?? 'http://localhost:8080').replace(/\/$/, ''),
};
```

The defaults (`http://localhost:...`) are used when no environment variable is set.
This is fine for development (backend running locally), but for distributing to other machines
pointing at a real server you need to hardcode the production addresses.

### Option A — Hardcode URLs before building (recommended for distribution)

Edit the defaults in `apiConfig.ts`:

```typescript
export const API_CONFIG = {
    USER_SERVICE:    (process.env.VITE_USER_API_URL    ?? 'https://your-server.com:5003').replace(/\/$/, ''),
    SCHEDULE_SERVICE:(process.env.VITE_SCHEDULE_API_URL ?? 'https://your-server.com:5002').replace(/\/$/, ''),
    KEYCLOAK_URL:    (process.env.VITE_KEYCLOAK_URL    ?? 'https://your-server.com:8080').replace(/\/$/, ''),
};
```

Then rebuild:

```bash
cd Front/electron-vue
npm run build
```

### Option B — Set environment variables at runtime (dev/testing)

Because the URLs are read from `process.env` in the Electron main process, you can override
them at launch without rebuilding by setting OS environment variables before starting the app.

**PowerShell (Windows):**
```powershell
$env:VITE_USER_API_URL     = "http://192.168.1.10:5003"
$env:VITE_SCHEDULE_API_URL = "http://192.168.1.10:5002"
$env:VITE_KEYCLOAK_URL     = "http://192.168.1.10:8080"
& ".\Расписание-Windows-0.0.0-Setup.exe"
```

This approach only works for the current shell session and is not practical for end users.
Use Option A when building an installer for others.

## What NOT to change

- Do not change `VITE_DEV_SERVER_URL` — that is set automatically by Vite during `npm run dev`.
- Do not add `VITE_*` variables to `.env` files expecting them to be baked in at build time.
  These variables live in the Electron main process (Node.js) and are read at runtime, not at
  Vite build time.

## Updating the app version

Change the `"version"` field in `Front/electron-vue/package.json` before building.
The installer filename and output folder use this version number.
