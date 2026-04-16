# Mission

Gary Tan Tycoon Simulator — a 3D tycoon game where typing generates Lines of Code, LOC earns Dollars, and Dollars buy Hustles that compound income. Single apartment scene. Optional slots room down the hall.

## Core Design Rules

- **LOC is the heartbeat.** Every dollar earned traces back to typing. Nothing earns passively until Gary has typed his way to it.
- **The apartment tells the story.** The 3D environment is the only narrative device — no cutscenes.
- **Hustles multiply, never replace.** Side income amplifies LOC earnings; ignoring them is still a complete game.
- **Gambling is optional.** The Slots Room is physically separated. Whatever happens there doesn't touch the main loop.

## Camera

- Overhead isometric Sims-style — fixed 50° pitch, player-controlled Y rotation (Q/E), WASD pan, scroll-wheel zoom.
- The camera **never zooms in or moves toward an item** when the player interacts with it.
- Item interaction surfaces a **UI panel overlay** — the overhead view stays locked at all times.

## Interaction Pattern

- Player proximity triggers interaction availability (approach an object).
- Interaction opens a **screen-space UI panel** — no camera cuts, no model close-ups.
- UI panel closes when the player steps away or dismisses it.

## Economy Constants

- Starting balance: **$47**
- LOC → Dollars: **10 LOC = $1** (`LOCToDollarRate = 0.1f`)
- Hustle income runs on a fixed passive tick interval (rate TBD per hustle)
