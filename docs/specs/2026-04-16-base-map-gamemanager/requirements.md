# Requirements — Base Map + GameManager

## Scope

**Included:**
- Blocked-out apartment room: floor, walls, ceiling geometry (no props)
- `GameManager.cs` singleton — owns all economy state
- Stub HUD displaying LOC count and Dollar balance on screen
- Starting balance of $47 (per spec opening line)
- `CameraController.cs` — overhead isometric Sims-style camera with pan, rotate, and zoom

**Not included:**
- Desk, computer, or any furniture props (Kaylee's ticket)
- Hustle corkboard or Hustle logic
- Typing input / LOC generation (separate ticket)
- Door or hallway to Slots Room
- Passive income ticks

### GameManager fields

| Field | Type | Initial value | Notes |
|-------|------|---------------|-------|
| `DollarBalance` | `float` | 47f | Per spec: "San Francisco. $47 in the bank." |
| `LOCCount` | `int` | 0 | Total LOC generated this session |
| `LOCPerSec` | `float` | 0f | Calculated from typing cadence (set by TypingInput later) |
| `LOCToDollarRate` | `float` | 0.1f | 10 LOC = $1 |

### Events

| Event | Signature | Fired when |
|-------|-----------|------------|
| `OnBalanceChanged` | `Action<float>` | DollarBalance changes |
| `OnLOCChanged` | `Action<int, float>` | LOCCount or LOCPerSec changes |

## Decisions

- **Event pattern (C# Actions):** HUD and future scripts subscribe rather than polling. Prevents per-frame coupling.
- **Singleton:** `GameManager.Instance` — standard Unity singleton via `Awake`. Only one instance enforced.
- **Stub HUD:** Text-only, no styling. Two `TextMeshPro` labels: LOC count + LOC/sec on one line, Dollar balance on another. Positioned top-left overlay. Full HUD polish is a separate ticket.
- **LOC→Dollar rate hardcoded:** `LOCToDollarRate = 0.1f` (10 LOC = $1). Exposed as a serialized field for designer tuning in Inspector.
- **Camera — Sims-style overhead:** Fixed 50° pitch, player-controlled Y rotation (Q/E), WASD pan, scroll-wheel zoom. Uses New Input System. No first-person or follow camera.
- **Item interaction:** When the player interacts with an item, a UI panel appears on screen. The camera does not zoom in or move toward the item — the overhead view stays locked.

## Context

- Unity 6.3 + URP — no UGUI canvas workarounds needed; TextMeshPro is the standard text solution
- `GameManager` must be in the scene (not Resources-loaded) so it shows in the Hierarchy for teammates
- Keep the room geometry simple ProBuilder or primitive GameObjects — art pass comes from Kaylee
- Stub HUD text style matches ambient/diegetic feel — no flashy UI chrome for this pass
