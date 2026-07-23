# Schedule · Desktop UI Kit

A high-fidelity recreation of the **Schedule** Electron + Vue desktop app, rebuilt as React/JSX components so they can be reused inside design mocks.

This kit replicates the visible UI; it does **not** wire up the backend. All data is mocked locally.

## Source of truth

Each component traces back to a file in `Schedule/Schedule/electron-vue/src/components/`:

| JSX file | Source `.vue` |
|---|---|
| `NavPill.jsx` | `static/Header.vue` |
| `SchedulePage.jsx` | `static/pages/Schedule.vue` |
| `Calendar.jsx`, `DayCell.jsx` | `paterns/scheduleComponents/Calendar.vue`, `Day.vue` |
| `DepartmentList.jsx` | `paterns/scheduleComponents/ScheduleDepartment.vue`, `DepartmentList.vue` |
| `DepartmentsPage.jsx`, `DepartmentCard.jsx`, `ProfileRow.jsx` | `pages/Departmets.vue`, `paterns/Department.vue`, `ProfileCard.vue` |
| `ControlPanelPage.jsx`, `Sidebar.jsx`, `AddUserForm.jsx` | `pages/ControlPanel.vue`, controlForms `AddUserForm.vue` |
| `FormInput.jsx`, `SelectInput.jsx`, `SubmitButton.jsx`, `FormHeader.jsx` | `common/*.vue` |

## How to run

Open `index.html` — it boots a clickable click-thru:
1. Lands on the **Schedule** view with the green nav-underline.
2. Pick a department → pick a doctor → see the month grid populate with mock shifts.
3. Click **отделения** → expandable department cards with profile rows.
4. Click **панель управления** → sidebar with color-coded actions; click "Добавить работника" to see the form.

## Conventions

- All copy is **Russian, lowercase** in source — CSS uppercases it.
- Components consume tokens from `../../colors_and_type.css`.
- Icons are referenced as `../../assets/icons/<name>.svg` (relative to the kit).
- No real fetches; mock data lives in `mockData.js`.
