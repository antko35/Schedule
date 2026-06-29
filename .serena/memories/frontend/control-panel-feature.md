## Control Panel Feature (Session 3 — complete)

### Route
`/control` → `ControlPanelPageComponent` (lazy, via `app.routes.ts` loadComponent)
No child routes file needed — form switching is in-component state, not router-based.

### Components tree
```
ControlPanelPageComponent (smart)       features/control-panel/components/control-panel-page/
  ├── SidebarComponent (dumb)           features/control-panel/components/sidebar/
  ├── WelcomeDashboardComponent (dumb)  features/control-panel/components/welcome-dashboard/
  ├── FormHeaderComponent (shared)      shared/components/form-header/
  ├── AddUserFormComponent (dumb)       features/control-panel/components/forms/add-user-form/
  ├── DeleteUserFormComponent (dumb)    features/control-panel/components/forms/delete-user-form/
  ├── UpdateUserFormComponent (dumb)    features/control-panel/components/forms/update-user-form/
  ├── CreateDepartmentFormComponent     features/control-panel/components/forms/create-department-form/
  ├── AddUserToDepartmentFormComponent  features/control-panel/components/forms/add-user-to-department-form/
  ├── DeleteDepartmentFormComponent     features/control-panel/components/forms/delete-department-form/
  └── PlaceholderFormComponent (dumb)   features/control-panel/components/forms/placeholder-form/
```

### Store: controlPanelStore (features/control-panel/store/control-panel.store.ts)
State: `{ activeForm, loading, error, successMessage, users, departments }`
Methods:
- `setActiveForm(route: string | null)` — plain, clears toasts
- `clearToast()` — plain, nulls error + successMessage
- `loadUsers()` — rxMethod, fills `users` from GET /User
- `loadDepartments()` — rxMethod, fills `departments` from GET /Department
- `submitAddUser(dto: NewUser)` — rxMethod → POST /User
- `submitDeleteUser(id: string)` — rxMethod → DELETE /User/{id}
- `submitUpdateUser(dto: UpdateUser)` — rxMethod → PUT /User
- `submitCreateDepartment(dto: NewDepartment)` — rxMethod → POST /Department
- `submitAddUserToDepartment(dto: AddUserToDepartment)` — rxMethod → POST /Department/addUserToDepartment
- `submitDeleteDepartment(id: string)` — rxMethod → DELETE /Department/delete/{id}

### API services used
- `UserApiService` (features/control-panel/services/user-api.service.ts)
  Uses `userManagementServiceUrl` (port 5003 proxied as /user-management-api)
  Methods: getUsers, createUser, updateUser, deleteUser
- `DepartmentApiService` (features/departments/services/) — **extended** in Session 3
  Added: createDepartment, deleteDepartment, addUserToDepartment

### Models (features/control-panel/models/)
- `func-list-item.model.ts` — FuncListItem { id, text, route, style: ActionStyle }
- `new-user.model.ts` — CreateUserCommand fields (all nullable)
- `update-user.model.ts` — UpdateUserInfoCommand incl. userId in body
- `new-department.model.ts` — { deartmentName (⚠️ swagger typo!), clinicId }
- `add-user-to-department.model.ts` — { userId, departmentId, role, status, email, phoneNumber }
- `control-panel-state.model.ts` — full state interface (imports from departments models)

### Data constant (features/control-panel/data/func-lists.const.ts)
Exports: `doctorFuncList`, `departmentFuncList`, `scheduleFuncList`, `FORM_META`

### Form routes and status
| route | Form Component | Status |
|---|---|---|
| addUser | AddUserFormComponent | ✅ implemented |
| deleteUser | DeleteUserFormComponent | ✅ implemented |
| redactUserInfo | UpdateUserFormComponent | ✅ implemented |
| createDepartment | CreateDepartmentFormComponent | ✅ implemented |
| addUserToDepartment | AddUserToDepartmentFormComponent | ✅ implemented |
| deleteDepartment | DeleteDepartmentFormComponent | ✅ implemented |
| redactUserInfoFromDepartment | PlaceholderFormComponent | 🔲 placeholder |
| deleteUserFromDepartment | PlaceholderFormComponent | 🔲 placeholder |
| createUserSchedule | PlaceholderFormComponent | 🔲 placeholder |
| transferDay | PlaceholderFormComponent | 🔲 placeholder |
| deleteHoliday | PlaceholderFormComponent | 🔲 placeholder |
| exportSchedule | PlaceholderFormComponent | 🔲 placeholder |

### Key patterns & gotchas
- `DepartmentUser` model (from departments feature) is reused as the User type —
  UserManagementService GET /User returns the same shape
- `deartmentName` field is a real typo in the swagger (CreateDepartmentCommand) — send it as-is
- PUT /User takes UpdateUserInfoCommand with userId in the body (not path param)
- Form switching is state-based (@switch on store.activeForm()), NOT router child routes
- Users/departments are loaded lazily via `effect()` in the page when activeForm changes
- Form reset on success: page uses `viewChild()` per form, calls `.reset()` in success effect
- Toast: `MessageService` provided in `ControlPanelPageComponent`, PrimeNG `<p-toast position="bottom-right">`
- Gender picker: two `.pill` buttons (not a select), stored as 'Мужской' / 'Женский'
- User list in delete/update forms: scrollable custom list (no PrimeNG Dropdown), max-height 200-260px

### Tests (Karma/Jasmine)
- `user-api.service.spec.ts` — HTTP mocking, 4 methods
- `control-panel.store.spec.ts` — fakeAsync + tick + TestBed.flushEffects()
- `add-user-form.component.spec.ts` — submit validation, emit, reset, gender
- `create-department-form.component.spec.ts` — submit validation, emit (deartmentName), reset
