# Requirements — Vibe Coding Computer

## Scope

**Included:**
- Computer desk placeholder (DeskBody + Monitor cubes) with proximity trigger
- `TypingInput.cs` — captures keypresses while player is in range, advances code display, drives LOC/sec
- `ComputerDisplay.cs` — IDE-style scrolling code feed, line numbers, persists file position between visits
- `ComboMultiplier.cs` — velocity-based LOC/sec: each keypress instantly bumps the rate, hard cutoff to 0 after 0.3s of no typing
- `ComputerProximity.cs` — SphereCollider trigger on Desk activates/deactivates typing on player entry/exit
- `PlayerController.cs` — movement frozen while at computer; Escape exits and re-enables movement
- GStack source code bundled as `Assets/Resources/GStackSourceCode.txt`
- "ESC to exit" label on code panel top-right

**Not included:**
- Multiple monitors (OpenClaw hustle ticket)
- Syntax highlighting or IDE theming (art pass)
- Persistent file position across Play sessions (resets on game start)

### Key Fields

| Field | Owner | Type | Notes |
|-------|-------|------|-------|
| `_corpus` | `ComputerDisplay` | `string` | Full GStack source loaded from Resources |
| `_charIndex` | `ComputerDisplay` | `int` | Position in corpus; persists between visits, wraps at end |
| `_lineNumber` | `ComputerDisplay` | `int` | Current line number; shown in gutter, resets on corpus wrap |
| `charsPerKeypress` | `ComputerDisplay` | `int` | Characters advanced per keypress (default: 3) |
| `Current` | `ComboMultiplier` | `float` | Current LOC/sec rate; uncapped |
| `bumpPerKeypress` | `ComboMultiplier` | `float` | LOC/sec added per keypress (default: 2f) |
| `stopThreshold` | `ComboMultiplier` | `float` | Seconds before hard cutoff to 0 (default: 0.3s) |
| `IsActive` | `TypingInput` | `bool` | True when player is within proximity trigger |

## Decisions

- **Velocity-based LOC/sec:** Each keypress instantly adds `bumpPerKeypress` to LOC/sec. After `stopThreshold` seconds of no typing, snaps to 0. No multiplier — uncapped and directly proportional to typing speed.
- **LOC count on newlines only:** `GameManager.AddLOC()` is called only when the corpus advances past a `\n`. One real line of code = one LOC.
- **Dollars continuous, LOC discrete:** Dollars flow every frame via `GameManager.Update()` based on LOCPerSec. LOC count increments on newlines. These are decoupled.
- **File position persists between visits:** `_charIndex` and `_lineNumber` are not reset on `Deactivate()`. Display buffer clears but picks up from the same position in the file on next visit.
- **Proximity — OnTriggerStay fallback:** Uses both `OnTriggerEnter` and `OnTriggerStay` to catch cases where the player starts inside the trigger zone.
- **Escape to exit:** Pressing Escape deactivates the display, resets LOC/sec to 0, and re-enables player movement.
- **LOC/sec HUD throttled:** Display updates every 0.5s, rounded to 1 decimal. Underlying rate updates every frame.
- **Line numbers:** Right-aligned 4-char gutter prepended to each line (`   1 `, `  42 `). Resets when corpus wraps.

## Context

- Follows mission.md interaction pattern: proximity triggers UI overlay, camera never moves
- Player tagged `CameraPivot` with `SphereCollider` (non-trigger) is the proximity detection target
- `ComboMultiplier` and `ComputerProximity` both live on the `Desk` GameObject
- No new packages — TextMeshPro handles monospace display
