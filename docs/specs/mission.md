# Mission

Gary Tan Tycoon Simulator — a 3D tycoon game where typing generates Lines of Code, LOC earns Dollars, and Dollars buy Hustles that compound income. Single apartment scene. Optional slots room down the hall.

## Core Design Rules

- **LOC is the heartbeat.** Every dollar earned traces back to typing. Nothing earns passively until Gary has typed his way to it.
- **The apartment tells the story.** The 3D environment is the only narrative device — no cutscenes.
- **Hustles multiply, never replace.** Side income amplifies LOC earnings; ignoring them is still a complete game.
- **Gambling is optional.** The Slots Room is physically separated. Whatever happens there doesn't touch the main loop.

## Camera

- Overhead isometric Sims-style — fixed 40° pitch, player-controlled Y rotation (Q/E), scroll-wheel zoom.
- Camera follows the player with a slight lerp drift — it does not pan independently.
- The camera **never zooms in or moves toward an item** when the player interacts with it.
- Item interaction surfaces a **UI panel overlay** — the overhead view stays locked at all times.

## Player

- WASD moves a player character around the room.
- Current placeholder: tall rectangle cube `(0.4 × 1.8 × 0.4)`.
- Proximity detection uses a `SphereCollider` on the player, not the camera.

## Interaction Pattern

- Player walks near an object to trigger proximity detection (`SphereCollider` + `OnTriggerStay`).
- Interaction opens a **screen-space UI panel** — no camera cuts, no model close-ups.
- UI panel closes when the player walks away.

## Economy Constants

- Starting balance: **$47**
- LOC → Dollars: **10 LOC = $1** (`LOCToDollarRate = 0.1f`)
- Hustle income runs on a fixed passive tick interval (rate TBD per hustle)
