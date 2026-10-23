## Departments Feature (Session 2 — complete)

### Components tree
```
DepartmentsPageComponent (smart)         — features/departments/components/departments-page/
  └── DepartmentCardComponent (dumb)     — features/departments/components/department-card/
        └── ProfileRowComponent (dumb)   — features/departments/components/profile-row/
```

### Store: departmentsStore (features/departments/store/departments.store.ts)
Provided in root via `signalStore({ providedIn: 'root' })`.
State: `{ departments, doctorsByDept, loading, error, searchQuery }`
Methods:
- `loadAll()` — rxMethod<void>: sets loading, calls getDepartments(), then forkJoin for all
  getDepartmentUsers() calls in parallel, stores results in one patchState
- `setSearchQuery(query: string)` — plain method, patchState
Computed:
- `filteredDepts` — filters by searchQuery against dept name AND doctor full name (client-side)

### API: DepartmentApiService (features/departments/services/department-api.service.ts)
- `GET /Department` → `Observable<Department[]>`
- `GET /Department/getUsersByDepartment/{id}` → `Observable<DepartmentUser[]>`
  Note: URL path has typo in swagger ("deparmtmentId") but the URL path itself is correct.
Base URL from `inject(ENVIRONMENT).userManagementServiceUrl`.

### Models (features/departments/models/)
- `department.model.ts` — `{ id, departmentName }`
- `department-user.model.ts` — `{ id, firstName, lastName, patronymic, gender, dateOfBirth, email, phoneNumber, role, status }` (all nullable except id)
- `departments-state.model.ts` — `DepartmentsState` interface

### Shared additions
- `shared/pipes/initials.pipe.ts` — `InitialsPipe` (takes DepartmentUser → 2-letter initials FL)
- `shared/utils/sort.utils.ts` — `groupByLetter<T extends { lastName }>()` returns `LetterGroup<T>[]`
  (array of `{ letter: string, items: T[] }`, sorted by lastName, localeCompare 'ru')

### Routing
- `features/departments/departments.routes.ts` — DEPARTMENTS_ROUTES, lazy-loads page component
- `app.routes.ts` uses `loadChildren` for /departments pointing to departments.routes.ts

### Key behaviors
- Search is client-side; accordion does NOT auto-open on search
- Letter grouping done in DepartmentCardComponent via `groupByLetter(doctors())`
- Russian pluralization: 1 работник / 2–4 работника / 5+ работников
- Chevrons use PrimeIcons (`pi-chevron-down`) with CSS rotation (closed = -90deg, open = 0deg)
- `--font-serif` token added to tokens.css (Georgia, 'Times New Roman', serif)
- `tapResponse` from `@ngrx/signals/rxjs-interop` is NOT available (only @ngrx/signals installed);
  use `tap` + `catchError` from rxjs instead

### Tests
Karma/Jasmine (devDependencies; CLAUDE.md says Jest but package.json is definitive):
- `department-api.service.spec.ts` — HTTP mocking with HttpClientTestingModule
- `departments.store.spec.ts` — uses TestBed.flushEffects() for rxMethod
- `initials.pipe.spec.ts` — pure unit, no TestBed needed
- `sort.utils.spec.ts` — pure unit, no TestBed needed
