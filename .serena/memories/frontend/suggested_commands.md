# Frontend Suggested Commands

Project lives in `FrontendAngular/`. Run all commands from that directory.

**Note:** Project not yet scaffolded — `package.json` / `angular.json` don't exist yet. Scaffold first with Angular CLI.

## Scaffold (first-time only)
```powershell
cd FrontendAngular
ng new schedule-frontend --standalone --routing --style=css
```

## Dev
```powershell
npm start          # or: ng serve
```

## Build
```powershell
ng build           # dev build
ng build --configuration production
```

## Tests
```powershell
ng test            # Jest (once configured)
```

## Lint / Format
```powershell
ng lint            # ESLint
npx prettier --write "src/**/*.{ts,html,css}"
```

## Install deps (after scaffold)
```powershell
npm install @ngrx/signals keycloak-angular keycloak-js primeng @primeng/themes
```
