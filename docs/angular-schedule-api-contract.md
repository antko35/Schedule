# Angular Schedule page API contract

This document records the API surface needed to migrate the legacy Electron/Vue
Schedule page to `apps/web`. It is based on the current controllers, application
models, validators, and the legacy UI call flow.

## Scope and conventions

- All listed endpoints require a bearer token because their controllers use
  `[Authorize]`.
- ASP.NET Core's default JSON naming is camel case.
- `ObjectId` means a 24-character MongoDB object-id string.
- Dates and date-times are JSON strings. Send date-times as ISO 8601 values with
  an explicit offset, preferably UTC (`2026-09-18T08:00:00Z`).
- `year` is valid from 2000 through 2100 and `month` is one-based (`1` through
  `12`) where the Schedule API validators enforce those constraints.
- A missing schedule/rules record is currently returned as `404` by the custom
  exception middleware. Validation failures are returned as `400`.

## Endpoints required for the first read-only page

### 1. List departments

`GET {userManagementServiceUrl}/Department`

Request: no parameters.

Response: `200 application/json`, `Department[]`.

| Field | JSON type | Required | Notes |
|---|---|---:|---|
| `id` | `string` | yes | Department MongoDB `ObjectId`. |
| `departmentName` | `string \| null` | yes | Nullable in the backend domain model. |
| `clinicId` | `string \| null` | yes | MongoDB `ObjectId` when present. Not needed by the Schedule UI. |

Legacy use: loads the department accordion when the page opens.

Angular status: already wrapped by `DepartmentApiService.getDepartments()` and
represented by `Department`, although the Angular model currently omits
`clinicId` and treats `departmentName` as non-null.

### 2. List employees in a department

`GET {userManagementServiceUrl}/User/{departmentId}/ShortUserInfo`

Path parameters:

| Field | Type | Required | Notes |
|---|---|---:|---|
| `departmentId` | `string` | yes | Department MongoDB `ObjectId`. The endpoint has no explicit validator. |

Response: `200 application/json`, `ShortUserInfo[]`.

| Field | JSON type | Required | Notes |
|---|---|---:|---|
| `id` | `string` | yes | User MongoDB `ObjectId`. |
| `firstName` | `string` | yes | Declared non-null by the response DTO. |
| `lastName` | `string` | yes | Declared non-null by the response DTO. |
| `patronymic` | `string` | yes | Declared non-null by the response DTO. |

Legacy use: loaded lazily when a department accordion is opened.

Angular status: not wrapped. The existing
`DepartmentApiService.getDepartmentUsers()` calls the larger
`GET /Department/getUsersByDepartment/{departmentId}` endpoint instead. Add a
dedicated short-user method/model for the Schedule page.

### 3. Load one employee's monthly schedule

`GET {scheduleServiceUrl}/Schedule/schedule/{userId}/{departmentId}/{year}/{month}`

Path parameters:

| Field | Type | Required | Constraints |
|---|---|---:|---|
| `userId` | `string` | yes | Valid MongoDB `ObjectId`. |
| `departmentId` | `string` | yes | Valid MongoDB `ObjectId`. |
| `year` | `number` (`int`) | yes | 2000-2100. |
| `month` | `number` (`int`) | yes | 1-12. |

Response: `200 application/json`, `Schedule`.

#### `Schedule`

| Field | JSON type | Required | Notes |
|---|---|---:|---|
| `id` | `string \| null` | yes | Schedule MongoDB `ObjectId`; nullable in the domain model. |
| `monthName` | `string` | yes | Lowercase English month name, for example `september`. |
| `workDays` | `WorkDay[]` | yes | Empty array means no planned workdays. |

#### `WorkDay`

| Field | JSON type | Required | Notes |
|---|---|---:|---|
| `day` | `number` (`int`) | yes | Day of month. |
| `startTime` | `string` | yes | ISO 8601 date-time with offset. |
| `endTime` | `string` | yes | ISO 8601 date-time with offset. |

Legacy use: calendar cells, shift classification, work-time display, and all
calculated hour statistics. The legacy client accepts either one `Schedule` or
an array, but the API contract is one `Schedule`; Angular should not preserve
that defensive union unless live API evidence contradicts the controller.

Angular status: no API method or models exist yet.

### 4. Load the selected employee's monthly generation rules

`GET {scheduleServiceUrl}/ScheduleRules/rules/{userId}/{departmentId}/{year}/{month}`

Path parameters:

| Field | Type | Required | Constraints |
|---|---|---:|---|
| `userId` | `string` | yes | Semantically a user `ObjectId`; this query currently does not validate it. |
| `departmentId` | `string` | yes | Semantically a department `ObjectId`; this query currently does not validate it. |
| `year` | `number` (`int`) | yes | 2000-2100. |
| `month` | `number` (`int`) | yes | 1-12. |

Response: `200 application/json`, `UserScheduleRules`.

#### `UserScheduleRules`

| Field | JSON type | Required | Notes |
|---|---|---:|---|
| `id` | `string \| null` | yes | Rules MongoDB `ObjectId`. |
| `userId` | `string` | yes | User MongoDB `ObjectId`. |
| `departmentId` | `string` | yes | Department MongoDB `ObjectId`. |
| `monthName` | `string` | yes | Lowercase English month name. |
| `year` | `number` | yes | Calendar year. |
| `hoursPerMonth` | `number` | yes | Monthly target used by the hours summary. |
| `maxHoursPerDay` | `number` | yes | Despite the Mongo field name `hoursPerDay`, JSON uses this property name. |
| `startWorkDayTime` | `string` | yes | .NET `TimeOnly`, normally `HH:mm:ss`. |
| `onlyFirstShift` | `boolean` | yes | Generation rule. |
| `onlySecondShift` | `boolean` | yes | Generation rule. |
| `evenDOW` | `boolean` | yes | Second shift on Mon/Wed/Fri; first shift on Tue/Thu. |
| `unEvenDOW` | `boolean` | yes | First shift on Mon/Wed/Fri; second shift on Tue/Thu. |
| `evenDOM` | `boolean` | yes | First shift on even days of month. |
| `unEvenDOM` | `boolean` | yes | First shift on odd days of month. |
| `scheduleId` | `string` | yes | Related Schedule MongoDB `ObjectId`. |

Legacy use: the old hours panel calls `GET /ScheduleRules/getAll`, downloads all
users' rules, and filters them in the UI. The new Angular page should use this
narrow endpoint instead. For a missing rule, the API returns `404`; the legacy
UI falls back to a hard-coded 168-hour norm, but that fallback is presentation
behavior rather than an API guarantee.

Angular status: no API method or model exists yet.

## Additional read endpoint that should not drive the first page

`GET {scheduleServiceUrl}/Schedule/schedule/{departmentId}/{year}/{month}`

It returns `Schedule[]`, but each element has no `userId` or `departmentId`.
Consequently, the frontend cannot reliably associate schedules with employees.
The legacy Schedule page does not use it. Use the per-user endpoint until the
backend returns an explicit user-to-schedule projection.

## Endpoints required for the later editing mode

These endpoints are used by the legacy `/redactSchedule` view. They are not
required to deliver the first read-only page.

### Create or replace a workday

`POST {scheduleServiceUrl}/Schedule`

Request body: `CreateWorkDayRequest`.

| Field | JSON type | Required | Constraints |
|---|---|---:|---|
| `userId` | `string` | yes | Valid MongoDB `ObjectId`. |
| `departmentId` | `string` | yes | Valid MongoDB `ObjectId`. |
| `startTime` | `string` | yes | ISO 8601 date-time. |
| `endTime` | `string` | yes | Same date as `startTime`; must be greater than or equal to it. |

Response: `200 application/json`, the created/replaced `WorkDay`.

### Delete a workday

`DELETE {scheduleServiceUrl}/Schedule`

Request body: `DeleteWorkDayRequest`.

| Field | JSON type | Required | Constraints |
|---|---|---:|---|
| `userId` | `string` | yes | Valid MongoDB `ObjectId`. |
| `departmentId` | `string` | yes | Valid MongoDB `ObjectId`. |
| `workDay` | `string` | yes | ISO 8601 date-time; its date identifies the day to remove. |

Response: `200 application/json`, `Schedule` as currently declared by the
handler. A missing schedule/day is returned as `404`.

### Add a working Saturday and shorten five existing workdays

`POST {scheduleServiceUrl}/Schedule/addSaturday`

Request body: `AddWorkingSaturdayRequest`.

| Field | JSON type | Required | Constraints |
|---|---|---:|---|
| `scheduleId` | `string` | yes | Existing Schedule MongoDB `ObjectId`; no format validator currently. |
| `saturday` | `WorkDay` | yes | `startTime` must fall on a Saturday. |
| `workingDays` | `WorkDay[]` | yes | Exactly five existing days; each adjusted day must be at least four hours. Their removed time must equal the Saturday duration. |

Response: `200` with no response body.

## Existing Angular coverage summary

| Capability | Present in `apps/web`? | Notes |
|---|---:|---|
| Department list | partial | Service/model exist; model nullability does not exactly match the backend. |
| Short employee list by department | no | Existing service uses the full-user endpoint. |
| Monthly schedule read | no | No service method or `Schedule`/`WorkDay` models. |
| Monthly rule read | no | No service method or `UserScheduleRules` model. |
| Generate department schedule | yes | Implemented in the Control Panel service/store. |
| Export department schedule | yes | Implemented in the Control Panel service/store. |
| Holiday add/delete | yes | Implemented in the Control Panel service/store. |
| Workday create/delete | no | Needed only for the later editing mode. |
| Working Saturday adjustment | no | Needed only for the later editing mode. |

## Recommended first-page call flow

1. Load departments once.
2. Load short employees lazily when a department is expanded.
3. When an employee is selected or the month changes, request that employee's
   `Schedule` and `UserScheduleRules` concurrently.
4. Treat a missing schedule/rules response as an empty/not-generated state;
   do not silently treat every server error as an empty month.
