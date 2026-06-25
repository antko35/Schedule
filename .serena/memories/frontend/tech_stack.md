# Frontend Tech Stack

| Tool | Version / Decision |
|---|---|
| Angular | 19, standalone components everywhere |
| State management | NgRx Signal Store (`@ngrx/signals`) — `signalStore` + `withState` + `withMethods` + `rxMethod` |
| UI library | PrimeNG |
| HTTP | Angular HttpClient |
| Auth | keycloak-angular + keycloak-js |
| Styles | CSS custom properties (design tokens from `styles/tokens.css`) |
| Tests | Jest |
| Linter/Formatter | ESLint + Prettier |
| Font | Inter (Google Fonts), weights 400/500/600/700 |

## Key constraints
- No NgModule — standalone components only
- No `any` in TypeScript — strict mode
- Signals everywhere (Angular 19 signals API)
- `inject()` for DI, not constructor injection
- Async pipe in templates, not `.subscribe()` in components
- All interfaces derived from swagger specs in `docs/`
