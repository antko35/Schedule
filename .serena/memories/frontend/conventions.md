# Frontend Conventions

## Naming
| Artifact | Convention |
|---|---|
| Component folder | `kebab-case/` |
| Component file | `component-name.component.ts` |
| API service | `feature-name-api.service.ts` |
| Store | `feature-name.store.ts` |
| Model | `model-name.model.ts` |
| CSS classes | `kebab-case`, scoped per component |

## Angular rules
- **Standalone components only** — no NgModule
- `inject()` for DI (not constructor)
- Signals over BehaviorSubject; async pipe in templates
- No `any` — strict TypeScript
- All interfaces derived from swagger specs

## NgRx Signal Store pattern
```typescript
export const DepartmentsStore = signalStore(
  { providedIn: 'root' },
  withState<DepartmentsState>({ departments: [], loading: false, error: null }),
  withMethods((store, api = inject(DepartmentApiService)) => ({
    loadDepartments: rxMethod<void>(
      switchMap(() => api.getDepartments().pipe(
        tapResponse({
          next: (departments) => patchState(store, { departments }),
          error: (error) => patchState(store, { error }),
        })
      ))
    ),
  }))
);
```

## HTTP service pattern
```typescript
@Injectable({ providedIn: 'root' })
export class DepartmentApiService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(ENVIRONMENT).userManagementServiceUrl;
  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.base}/Department`);
  }
}
```

## Error handling
- `ErrorInterceptor` catches all HTTP errors globally — no per-component handling
- Store holds `error: string | null`; component shows via PrimeNG Toast
- `AuthInterceptor` attaches `Authorization: Bearer <token>` automatically

## Forbidden
- `any` in TypeScript
- `subscribe` inside `subscribe`
- Business logic in components
- Hardcoded URLs (use `environment`)
- Direct HTTP calls from components
- NgModule

## CSS design tokens (from `styles/tokens.css`)
Backgrounds: `--bg-app #111111`, `--bg-elevated #1c1c1c`, `--bg-surface #1f1f1f` (+ `-2/-3/-4`)  
Text: `--fg #e8e8e8`, `--fg-quiet #888`, `--fg-muted #666`, `--fg-strong #fff`  
Section accents: `--accent-schedule #4fc3f7`, `--accent-departments #d8ff2b`, `--accent-control #7c6af7`  
Actions: `--action-add #04ff00`, `--action-delete #ff3b3b`, `--action-redact #f6ff00`  
Sidebar dot = action color (add→green, delete→red, redact→yellow)

## Task workflow (before writing code)
1. Read relevant `mem:frontend/*-feature` memory
2. Read corresponding swagger.json in `docs/`
3. Write a plan — ask if unclear
4. Implement: models → api service → store → components (smart→dumb) → tests → memory update
