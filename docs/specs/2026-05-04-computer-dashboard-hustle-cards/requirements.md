# Requirements — Computer Dashboard Hustle Cards

## Scope

**Included:**
- Computer dashboard UI layout that combines:
  - Existing Vibe Code entry point (already implemented behavior)
  - Hustle cards section for dashboard management
- Hustle card visual states for this phase:
  - `Vibe Code` card shown as active
  - `Bug Bounty` card shown as active
  - `OpenClaw` card shown as locked gray "Purchase Hustle"
  - `Airbed` card shown as locked gray "Purchase Hustle"
- Navigation polish for dashboard interactions:
  - Keyboard focus traversal
  - Controller focus traversal
  - Visible focused/selected states
- Button wiring limited to existing Vibe Code behavior only

**Not included:**
- New hustle purchase logic, economy deduction, unlock rules, or passive income
- New Vibe Code gameplay logic (already implemented and reused)
- New backend/data persistence systems
- Final art polish, animation polish, or VFX polish

### Dashboard card behavior table

| Card | Default state | Button behavior (this phase) |
|------|---------------|-------------------------------|
| Vibe Code | Active | Uses existing implemented behavior |
| Bug Bounty | Active | UI-only (no new logic behind button) |
| OpenClaw | Locked gray "Purchase Hustle" | UI-only (no new logic behind button) |
| Airbed | Locked gray "Purchase Hustle" | UI-only (no new logic behind button) |

## Decisions

- **Computer is the hustle manager:** Hustles are viewed and managed from the computer dashboard, aligning with `project-spec.md`.
- **Vertical slice fidelity:** Functional layout + interactions first, minimal visual polish.
- **Visual lock pattern:** `OpenClaw` and `Airbed` communicate unavailable state via gray locked purchase cards.
- **Navigation-first UX polish:** Keyboard/controller focus states are in scope to support non-mouse flows.
- **Logic boundary:** Only Vibe Code keeps interactive behavior in this phase; all other hustle card actions remain UI placeholders.

## Context

- Follow mission and design rules in `docs/specs/mission.md`:
  - UI overlays for interaction, no camera movement changes
  - LOC-driven economy remains the core progression model
- Follow stack constraints in `docs/specs/tech-stack.md`:
  - Unity 6.3, URP, New Input System
  - No new packages/dependencies
- Keep this feature independently shippable as a UI milestone:
  - Dashboard communicates intended progression states clearly
  - Future ticket can add purchase/economy logic without reworking layout
