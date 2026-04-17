# Requirements — Vibe Coding Computer

## Scope

**Included:**
- Computer desk placeholder object in the apartment with a proximity trigger
- `TypingInput.cs` — captures any keypress while player is in range, advances code display, updates combo multiplier
- `ComputerDisplay.cs` — IDE-style scrolling code feed UI panel (screen-space overlay), renders GStack source code character by character
- `ComboMultiplier.cs` — tracks typing cadence, maintains a streak multiplier (1×–3×) that scales LOC/sec; decays when player slows or stops
- GStack source code bundled as `Assets/Resources/GStackSourceCode.txt` — used as the code feed corpus
- `GameManager.SetLOCPerSec()` called live as multiplier changes
- Proximity auto-focus: entering trigger activates display + keyboard capture; leaving deactivates both

**Not included:**
- Actual Gary player character or 3D movement (camera panning stands in for movement for now)
- Multiple monitors (OpenClaw hustle ticket)
- Syntax highlighting or IDE chrome/theming (art pass)
- Persistent LOC/sec across sessions

### Key Fields

| Field | Owner | Type | Notes |
|-------|-------|------|-------|
| `corpusText` | `ComputerDisplay` | `string` | Full GStack source loaded from Resources |
| `charIndex` | `ComputerDisplay` | `int` | Current position in corpus; wraps at end |
| `charsPerKeypress` | `ComputerDisplay` | `int` | Characters advanced per keypress (default: 3) |
| `multiplier` | `ComboMultiplier` | `float` | Current streak multiplier, 1.0–3.0 |
| `baseLocPerSec` | `ComboMultiplier` | `float` | LOC/sec at 1× (default: 5f) |
| `decayDelay` | `ComboMultiplier` | `float` | Seconds of no typing before decay begins (default: 1.5s) |
| `isActive` | `TypingInput` | `bool` | True when player is within proximity trigger |

## Decisions

- **Scrolling code feed styled like an IDE:** Dark background panel, monospace font, lines scroll upward as new ones appear. No syntax highlighting this pass — plain white text on dark bg.
- **Combo multiplier (1×–3×):** Typing cadence over a 1-second rolling window drives the multiplier. Fast sustained typing → 3×. Slowing past threshold → decays back toward 1× over 1.5s. `LOCPerSec = baseLocPerSec * multiplier` is pushed to `GameManager` every frame.
- **GStack source as corpus:** Bundled as a plaintext `.txt` asset. Cycles continuously — when `charIndex` reaches end, wraps to 0. No network fetch needed.
- **Proximity auto-focus:** A `SphereCollider` (trigger, radius ~2m) on the desk detects the camera pivot GameObject entering/exiting. On enter: UI panel activates, `TypingInput.isActive = true`. On exit: panel deactivates, `LOCPerSec` resets to 0.
- **Per-keypress corpus advance:** Each keypress moves `charIndex` forward by `charsPerKeypress` (3). The newly revealed characters are appended to the display buffer. When a newline is hit, it creates a new line in the scroll view.

## Context

- Follows the established interaction pattern: proximity triggers UI overlay, camera never moves
- `TypingInput` calls `GameManager.AddLOC()` per keypress AND updates `LOCPerSec` via `ComboMultiplier` — these are separate concerns
- The camera pivot GameObject (or a dedicated invisible "player" marker) is the proximity detection target
- GStack source corpus is bundled as `Assets/Resources/GStackSourceCode.txt`
- No new packages — TextMeshPro (already available via UGUI package) handles the monospace display
