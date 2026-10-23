# Schedule — Design System

A design system reconstructed from the **Schedule** project — a Russian-language dental clinic schedule manager built as an Electron + Vue 3 desktop application backed by .NET 10 microservices.

---

## Sources

- **Codebase** (mounted via Import): `Schedule/` — primary source of truth
  - Frontend (UI lifted from here): `Schedule/Schedule/electron-vue/`
  - Backend microservices: `Schedule/src/{UserService, ScheduleService, UserManagementService}` (.NET, MongoDB, Postgres, RabbitMQ, gRPC, Hangfire)
  - Architecture notes: `Schedule/CLAUDE.md`
  - Operating manual: `Schedule/README.md`
- **GitHub repos** referenced in the brief: `SonicKKO/Schedule`, `antko35/Schedule` (browsable via GitHub tooling on request — not yet imported)
- **No Figma file** was provided.
- **No slide deck or marketing material** was provided.

> Everything in this design system was reverse-engineered from the live Vue components in `Schedule/Schedule/electron-vue/src`. Tokens, surface colors, radii, fonts and component shapes are quoted directly from the implementation.

---

## What the product is

A single-screen desktop tool for the office manager of a small clinic to:

1. **Browse the monthly schedule** — pick a department, pick a doctor, see a 7-column month grid with shift labels (1st shift = green stripe, 2nd shift = orange stripe, weekend = dark red, today = lighter cell).
2. **Edit one doctor's month** — toggle shifts, transfer working Saturdays, mark official holidays.
3. **Manage staff and departments** — accordion of departments → grouped-by-first-letter doctor lists → full profile cards.
4. **Use the control panel** — forms grouped into Работники / Отделения / Расписание, each routed under `/controlpanel/<form>`. Add / delete / redact actions are color-coded in the sidebar (green/red/yellow).

The auto-generation of schedules happens server-side (Hangfire background jobs on `ScheduleService`); the UI is mostly read + manual edit.

---

## Surfaces / "products" covered

There is **one product surface**: the Electron desktop app. No website, no mobile, no admin web console exists in the codebase. The UI kit therefore mirrors that single surface:

- `ui_kits/desktop/` — Vue → JSX recreations of the three main screens plus the sidebar control panel.

---

## CONTENT FUNDAMENTALS

The product copy is **terse, Russian, almost telegraphic**. There is no marketing voice — this is an internal back-office tool written by and for one or two clinic staff.

**Language**
- All UI copy is in **Russian**. Examples lifted from code: `расписание`, `отделения`, `панель управления`, `Укажите работника`, `редактировать`, `Добавить работника`, `Удалить работника`, `Изменить информацию`, `Составить расписание работнику`, `Добавить официальный выходной`.
- Error/empty states are short and direct: `Ошибка запроса`, `Ошибка загрузки отделений`, `Нет добавленных работников`, `Загрузка`.
- Success copy uses the verb in past tense: `<Name> успешно добавлен(а)` — note the bracketed gendered ending `(а)`. Keep this pattern.

**Casing**
- Navigation, headings, sidebar groups and form titles are **all lowercase** in source but rendered **UPPERCASE** via `text-transform: uppercase`. So writers should author in lowercase; the CSS does the shouting.
  - Source: `<p class="schedule-p">расписание</p>` → renders as `РАСПИСАНИЕ`.
- Labels follow the same pattern: `label="Фамилия"` is written sentence-case in code, but `text-transform: uppercase` makes it `ФАМИЛИЯ`.
- Body and form values are **capitalize**d via CSS (`text-transform: capitalize`) — but the value-validation layer also calls `capitalize()` on the raw string in `FormUtils.ts`.

**Address**
- The tool addresses the user in the **imperative**, neutral form: `Укажите работника` ("Specify a worker"), not `ты` or `вы`. Verbs in form titles are infinitives: `Добавить`, `Удалить`, `Изменить`.
- The user refers to themselves implicitly — there is no "I" voice anywhere.

**Tone & vibe**
- **Functional, unembellished, slightly utilitarian** — feels like a hand-rolled tool for a single clinic, not a polished SaaS. Comments in code drift into Russian developer slang (`мб у папы на ффлешке`).
- No exclamation marks. No emoji in UI labels.
- Emoji **are** used as inline legend keys in one place (`🟩 - 1 смена`, `🟧 - 2 смена`, `🟥 - выходной`, `⬜ - текущий день`). Treat this as the only sanctioned emoji surface — a legend, not decoration.

**Numbers, abbreviations, formatting**
- Dates: `ДД.ММ.ГГГГ` (Russian DMY) shown to the user; ISO sent to server (`formatDateForServer` in `FormUtils.ts`).
- Phone: free text, prefix `+375` (Belarus) in code samples.
- Abbreviated profile labels: `ПОЛ`, `ИМЯ`, `ФАМИЛИЯ`, `ОТЧЕСТВО`, `Д/Р`, `ПОЧТА`, `ТЕЛЕФОН` — bold, colon, value. Keep as a fixed-width key, free-width value.

---

## VISUAL FOUNDATIONS

A summary of the visual DNA — the rules a new design must follow to feel like Schedule.

### Color vibe
Dark, slightly warm neutral (`#2a2a2a` body, `#383837` raised). Accents are **fully saturated near-primary** — pure green, pure yellow, pure red, pure blue. No pastels. No tints. Section-level identity is communicated by a **single accent line under the nav pill** that swaps color when you change pages.

### Type
- One typeface only: **Inter**, falling back to system-ui / Avenir / Helvetica / Arial. There are no font files in the repo — Inter is requested by name only, so the OS substitutes. *Substitution note:* the design system uses **Google Fonts Inter** so the preview matches what the app would look like on a fresh Linux/Windows box.
- One serif exception: `Georgia` is used **only** for the Cyrillic letter dividers in the doctor list (А, Б, В…). It's a deliberate big-display serif moment against an otherwise sans UI.
- All headings and labels are `text-transform: uppercase`. Values and free text are `text-transform: capitalize` or untransformed.

### Spacing
- Loose vertical rhythm. Forms use `gap: 30px` between rows. Sidebar lists use `gap: 50px` for the nav pill links.
- Right-margins on the calendar are generous (`60vw` content, ~10–20px gutters).
- Layouts are **fixed-pixel-ish** — `min-width: 702px`, `min-width: 843px`, `min-height: 750px`. This is a desktop app, not a responsive web.

### Backgrounds, imagery, illustration
- **No imagery.** No photos, no illustrations, no patterns, no gradients. The app is pure flat fill.
- The only "art" is a handful of utility SVG icons (chevron, arrow, avatar, weekend, search, etc) — single-stroke, single-color, no fill detail.

### Animation
- All motion is short and `ease`. Three durations dominate: `100ms` (tap), `200ms` (hover scale-up), `300ms` (border-color swap, fade-in).
- One slow exception: the department-card chevron rotates over `800ms` — a deliberate "unfolding" feel for the expand action.
- Pattern: **hover = scale(1.05–1.08) + brighter background**. Press = no distinct active state (relies on the `:hover` lift).
- One keyframe animation, reused: `fadeIn` — `opacity 0 → 1` while `max-height: 0 → 200–550px`. Used for accordion reveal.

### Hover / press states
- Hover on a **link** / nav item: lighten color from `#c4c4c4` → `#ffffff`, `transform: scale(1.08)`, underline.
- Hover on a **row** (sidebar li, profile li, department row): swap background to a lighter grey (e.g. `#717171` or `#a8a8a8`).
- Hover on an **icon button**: `opacity 0.6 → 1` and `scale(1.08)`. No color change.
- Hover on the close-X icon: background red brightens from `#ff2626` → `#ff0000`.
- **No explicit `:active` styles.** The app does not visually depress.

### Borders
- 1px solid `rgba(120,120,120,1)` separator on the sidebar. 1px solid `#616161` under profile fields. 1px dashed `rgb(117,113,113)` between year/month strips in the calendar.
- The **only** decorative border is the colored underline on the nav pill (`border-bottom: 3px solid <accent>`). Every other surface relies on background-fill alone to separate.

### Shadows
- **Zero shadows.** No `box-shadow` exists anywhere in the codebase. Elevation is signaled by background lightness, not depth.

### Transparency / blur
- Used once: `rgba(173,166,166,0.19)` for empty calendar cells (the leading/trailing days that belong to other months). Blur is never used.

### Corner radii
- Heavily rounded everything. Inputs are `4px` (the only sharp things). Day cells `15px`. Cards and form rows `20px`. Tap-target chevrons `30px`. The top nav pill is `35px`. Profile rows and rounded buttons `20px`.

### "Card" treatment
- Background fill only. No border, no shadow. Hover brightens the fill by one step.
- Department card: `bg #555555 + radius 20px`. Profile row: `bg #494949 + radius 20px`. Sub-row hover: `bg #717171`.

### Layout rules
- Strong center-axis layout. `margin-inline: auto` on almost every container.
- Pages have a sticky top nav pill, then either a two-pane (`choose-container` 270–390px + content) or single-pane center layout.
- Calendars and grids use CSS Grid (`grid-template-columns: repeat(7, 1fr)`). Forms use Flex column with `gap: 30px`.

### Use of `user-select`
- Globally disabled. The whole UI feels like a "native" app — no draggable text. Worth keeping in any web mock.

### Loading & success
- Loading: an inline 20×20 spinner inside the disabled submit button (`border 3px solid #f3f3f3; border-top #3498db`). Toast-style success message appears under the form for 5s and hides itself.

### Accent rules
- Each top-level route owns one accent color, used **only on the nav underline**, never in body content. Body uses the green/orange/red shift colors instead.

---

## ICONOGRAPHY

The app ships with **15 hand-rolled SVG icons** in `electron-vue/src/assets/` (now mirrored to `assets/icons/` here). They are:

- `chevron`, `left-arrow`, `right-arrow`, `arrow-up`, `arrow-down` — directional
- `menu`, `close-menu`, `search`, `scale`, `redact`, `redact-schedule` — actions
- `avatar`, `weekend`, `no-result`, `success` — illustrative

**Style:** All are **monochrome single-stroke or single-fill black SVGs** with no detail, intended to be rendered against a dark background (often at ~60% opacity, brightening to 100% on hover). They are essentially open-source iconpack mediums — `svgrepo.com` filenames are still visible in the original folder.

**Usage rules:**
- Icons are rendered as `<img src="…svg">` — not inlined. They cannot be CSS-recolored.
- Chevrons rotate (`transform: rotate(180deg)` ↔ `rotate(0deg)`) to indicate expand state — never swap icon, just rotate.
- Icon hit area is achieved with `padding` (e.g. `padding: 12px` on a 30px icon → 54px hit) rather than a wrapping button.
- Hover lifts opacity from `0.6 → 1` and scales `1.08`.

**Emoji** — used **once**, as a legend on the schedule page (`🟩 🟧 🟥 ⬜`). Not used elsewhere.

**No icon font.** No Lucide, no Heroicons, no FontAwesome. If you need an icon the codebase doesn't ship, prefer to **add a new SVG to `assets/icons/` in the same monochrome black style** rather than introducing a CDN icon set — flag the addition.

---

## Index of this design system

```
README.md                    ← you are here
SKILL.md                     ← cross-compatible Agent Skill manifest
colors_and_type.css          ← CSS custom properties + base selectors

assets/
  icons/                     ← 15 SVGs from the Vue app

preview/                     ← Design System tab cards (700×<varies>)

ui_kits/
  desktop/
    README.md
    index.html               ← clickable click-thru of the desktop app
    *.jsx                    ← React components (one per UI surface)
```

There are **no slide templates** because the project doesn't ship any.

---

## Caveats / open questions

- **No font files.** `Inter` is declared by name only. Loaded from Google Fonts in previews; flag to the user if a brand-specific font should replace it.
- **No logo.** Schedule has no marketing identity, no app icon shipped in the renderer (`public/icons/` is empty in the listing). A placeholder wordmark is used in previews.
- **All copy is Russian.** Components keep the original strings — do not translate without checking with the user.
- **GitHub repos `SonicKKO/Schedule` and `antko35/Schedule`** were referenced but not fetched; if they contain newer designs or branding, re-run the system after importing them.
