# Requirements — Computer Dashboard UI

## Scope

**Included:**

- Computer dashboard UI panel that is opened from Gary's computer interaction flow
- Dashboard card layout for:
  - `Vibe Code`
  - `Bug Bounty`
  - `OpenClaw`
  - `Airbed`
- Card state styling for this slice:
  - `Vibe Code` and `Bug Bounty` appear active
  - `OpenClaw` and `Airbed` appear locked with grayed-out "Purchase Hustle" card/button state
- Navigation-ready UI behavior (keyboard/controller focus order, selected/hover visual state)
- Button behavior for this slice:
  - `Vibe Code` keeps existing implemented behavior
  - Other cards are UI placeholders (no hustle economy logic yet)

**Not included:**

- New hustle business logic (cost deduction, ownership state persistence, passive income ticks)
- Unlock progression logic beyond static locked UI styling
- New gameplay systems for Bug Bounty, OpenClaw, or Airbed
- Data persistence/save system changes

### Dashboard Card Data (UI Model)

| Field | Type | Example | Notes |
|-------|------|---------|-------|
| `CardId` | `string` | `openclaw` | Stable internal identifier |
| `Title` | `string` | `OpenClaw` | Displayed as card heading |
| `Description` | `string` | `AI coding agents` | Short card subtitle |
| `CostLabel` | `string` | `$1,000` | Display-only in this slice |
| `CardState` | `enum` | `Locked` | `Active` or `Locked` |
| `ButtonLabel` | `string` | `Purchase Hustle` | Locked cards use purchase label |
| `ButtonEnabled` | `bool` | `false` | Locked cards disabled in this slice |
| `ActionType` | `enum` | `OpenVibeCode` | `OpenVibeCode` or `Placeholder` |

## Decisions

- **UI-only vertical slice:** This feature ships dashboard structure and interaction affordances without introducing new hustle systems, to keep scope independently shippable.
- **Static locked styling for OpenClaw/Airbed:** Both cards are gray and disabled with "Purchase Hustle" text in this phase, matching the project spec while avoiding premature unlock logic.
- **Vibe Code remains wired:** Existing Vibe Code behavior is preserved so the dashboard integrates with already implemented computer gameplay.
- **Navigation polish included:** Keyboard/controller focus traversal and selected-state visuals are in-scope so the dashboard can be used without mouse-only interaction.
- **Managed from computer only:** Hustle viewing/management entrypoint is the computer dashboard, not a wall board.

## Context

- Follow `docs/specs/mission.md`: interactions stay UI-overlay driven from in-world proximity; no camera cuts or cinematic transitions.
- Follow `docs/specs/tech-stack.md`: Unity 6.3, URP, Input System, no new packages.
- Follow `docs/specs/project-spec.md`: hustles are managed from the computer dashboard, and OpenClaw/Airbed use grayed locked purchase states.
- Tone and visual direction: clean, readable dashboard in the apartment's dim-tech vibe; clarity over heavy art polish for this slice.
