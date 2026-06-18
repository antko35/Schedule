---
name: schedule-design
description: Use this skill to generate well-branded interfaces and assets for Schedule, the Russian-language clinic schedule manager (Electron + Vue desktop app), either for production or throwaway prototypes/mocks/etc. Contains essential design guidelines, colors, type, fonts, assets, and UI kit components for prototyping.
user-invocable: true
---

# schedule-design

Read the `README.md` file within this skill, and explore the other available files:

- `colors_and_type.css` — drop-in CSS custom properties; import once and reference `var(--bg-app)`, `var(--accent-schedule)`, etc.
- `assets/icons/` — the full SVG icon set lifted from the Vue app; reference as `<img src="assets/icons/chevron.svg">`. Icons are monochrome and rotate, they do not recolor.
- `ui_kits/desktop/` — JSX recreations of the three main screens (Schedule, Departments, ControlPanel). Each component is a thin visual recreation, not production code.
- `preview/` — small sample cards that visualize tokens.

If creating visual artifacts (slides, mocks, throwaway prototypes, etc), copy assets out and create static HTML files for the user to view. Re-use the JSX from `ui_kits/desktop/` rather than re-deriving the UI from the codebase.

If working on production code, copy assets and read the rules in `README.md` to become an expert in designing with this brand.

If the user invokes this skill without any other guidance, ask them what they want to build or design, ask some questions, and act as an expert designer who outputs HTML artifacts _or_ production code, depending on the need.

## Brand quick-reference

- **Language:** Russian. Author copy lowercase; CSS uppercases via `text-transform: uppercase`.
- **Tone:** terse, imperative, internal-tool. No marketing voice, no exclamation marks, no decorative emoji.
- **Color:** dark warm-neutral surfaces (`#2a2a2a`, `#383837`, `#494949`, `#555`). One saturated accent line per route (green / yellow / blue / yellow).
- **Type:** Inter only, plus Georgia for the letter dividers in doctor lists.
- **Shape:** very rounded (radius 4 for inputs, 15 for day cells, 20 for cards, 35 for the nav pill).
- **Motion:** 100–300ms ease, scale(1.05–1.08) on hover, fade-in for reveals, 800ms slow chevron rotate.
- **No shadows, no gradients, no imagery.**
