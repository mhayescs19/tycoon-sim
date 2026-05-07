### Gary Tan Tycoon Simulator

#### 1. Purpose

Gary Tan Simulator is a 3D tycoon game built in Unity where the player starts as a young engineer named Gary in a run-down apartment on **Alcatraz Island** (San Francisco Bay). The core loop is typing-driven to generate Lines of Code: Gary writes code, earns money from his output, activates side hustles to compound income, and can disappear down the hall into a mysterious apartment to gamble at his own risk.

#### 2. Design Principles

- **Lines of Code are the heartbeat.** Every dollar earned and every unlock triggered traces back to LOC output. Nothing earns passively until Gary has typed his way to it.
- **The apartment tells the story.** The 3D environment is the only narrative device — no cutscenes needed.
- **Hustles multiply, never replace.** Side income streams amplify LOC earnings. A player who ignores them entirely still has a complete game loop.
- **Gambling is optional.** The Slots Room is physically separated. Players choose to enter. Whatever happens in there doesn't touch the main loop.

#### 3. Core Concepts

### Lines of Code (LOC)

The base unit of player output and the only primary currency. The player types any key on their keyboard to generate LOC. Typing speed directly determines LOC/sec. LOC converts to Dollars at a fixed rate. LOC count and current LOC/sec are always visible on the HUD.

### Apartment

The single 3D environment where all primary gameplay takes place. The unit sits **on Alcatraz Island** — think a cramped, improvised flat carved out of historic prison or staff infrastructure: dim light, thin walls, a window that hints at the Bay, fog, and rock. As Gary earns Dollars, the apartment visually upgrades — better furniture, more monitors, cleaner walls (including **wall texture** swaps per tier). Physical objects appear in the apartment as Hustles are purchased. A door leads to the hallway and the Slots Room down the hall.

### Hustle

A computer dashboard is the single place where Hustles are viewed and managed. The dashboard displays hustle cards, handles purchases, and is the only place Gary spends Dollars on Hustles. Each Hustle has a one-time cost, generates passive income once purchased, and spawns a physical object in the apartment.

Default hustle cards shown on the computer dashboard:
- Vibe Code
- Bug Bounty
- OpenClaw
- Airbed

OpenClaw and Airbed start as locked options in the dashboard and include grayed-out "Purchase Hustle" cards/buttons until the player has enough money to unlock them.

- Freelance Bug Bounty — always available. Lowest cost. Generates a flat passive Dollar/sec. Spawns a sticky note on the wall.
  - free hustle
  - sticky notes spawn onto the wall every 15 seconds; a ui tooltip appears to note that the user must claim the note
  - complexity the metric the user can see
  - the user must approach and activsate the bug bounty
  - complte the dead by daylight style generator check
- Airbed — sublets floor space. Generates passive Dollar/sec per unit. Supports up to 3 units at increasing cost and diminishing return per unit. Spawns a blow-up mattress and pillow on the floor per unit purchased, each with distinct **airbed / bedding textures** (vinyl, seams, valve read; pillow fabric) so multiple units read clearly in the room.
  - low passive money boost for each unit activated
- OpenClaw via Hostinger — highest cost. Runs an AI coding assistant that generates passive Dollar/sec and passive LOC/sec. The only Hustle that boosts LOC output. Spawns a glowing terminal window on a new monitor on the desk.
  - monopoly style progression: buy the first open claw for a lot, which activates a _x passive loc increase; then each openclaw agent will cost exponentially, eventaully you have 100x passive loc
    - passive loc is tied to the number of skills per agent
  - buy skills which boosts the active loc when typing
  - activates the server table (toggle on the model): buy more mac minis which boosts the passive loc

### Slots Room

Apartment 168 — a lucky casino number — down the hall from Gary's unit in the same Alcatraz building. Gary can play slots where wins and losses immediately update the game's main Dollar balance.

#### 4. User-Facing Behavior

- **Session start.** Player lands inside the apartment on Alcatraz. Gary's computer is on the desk. LOC count and LOC/sec are visible as ambient HUD elements. A single line sets the scene: "The Rock. $47 in the bank. One idea."
- **Core typing loop.** Player types any key to generate LOC in real time. Typing speed determines LOC/sec. LOC convert to Dollars at a fixed visible rate on the HUD. Dollars are spent from the computer dashboard to activate Hustles.
- **Computer hustle dashboard.** Player interacts with Gary's computer to open the dashboard. Hustles are shown as cards (Vibe Code, Bug Bounty, OpenClaw, Airbed) with name, cost, and effect. OpenClaw and Airbed show grayed-out "Purchase Hustle" cards/buttons while locked. Purchasing deducts Dollars immediately and spawns the Hustle's object in the apartment. Hustles are managed from this computer dashboard.
- **Slots Room.** Player walks to the hallway and enters Apartment 168. Player sets a wager from their current Dollar balance and pulls the lever. The slots spin and resolve immediately — win, lose, or jackpot — and the balance updates. Player can pull again or leave.
- End goal is becoming CEO of YCombinator which is an expensive buyout

#### 5. Art Assets

Art supports the apartment-as-story principle: readable silhouettes, clutter that reads as “early founder grind,” and upgrade states that feel earned rather than decorative.

**Environment**

- **Setting — Alcatraz Island** — apartment block or converted wing on the Rock; optional exterior shell or skybox reads Bay water, fog, distant city or bridges as art direction allows. Ground the fantasy with brutalist / aged plaster or painted concrete cues consistent with the island.
- **Main apartment** — one-room walk-up: walls, floor, door to hallway, window light as appropriate. **Wall textures** — baseline grime/peeling or flat institutional paint; upgrade tiers swap to cleaner paint, subtle trim, or warmer plaster normal/detail maps (coordinate with Dollar-driven furniture and lighting).
- **Hallway** — short connector from the apartment door to the Slots Room; should feel transitional (narrow prison-corridor or dingy motel-hall read), not a second hub.

**Interactive props**

- **Desk setup** — desk, chair, keyboard-visible workstation; room for extra monitors as hustle/unlock state.
- **Computer screen** — readable hustle dashboard framing (cards, states for locked vs purchasable) without prescribing final UI art; 3D bezel/screen mesh for in-world use.
- **Hustle spawns** — sticky notes on wall (Bug Bounty); blow-up mattress and pillow per Airbed unit with **airbed textures** (inflated vinyl, creases, seam tape, air valve) and **bedding textures** (pillow case, contrast) so 1–3 units stay legible; glowing terminal + additional monitor for OpenClaw; any Vibe Code–associated prop defined in implementation.
- **Bug Bounty interaction** — note mesh or decal cluster plus clear “claimable” read at play distance; optional generator/minigame props if those remain 3D.
- **OpenClaw / server table** — toggleable “server desk” or rack of Mac-mini–style units that can show more hardware as purchases stack.

**Slots Room (Apartment 168)**

- **Casino read** — slot machine body, lever, reels or reel surfaces, jackpot/winline affordances; lighting distinct from the apartment (slightly luck/tension, not full fantasy).

**Characters**

- **Gary** — third-person playable avatar consistent with “young engineer” (generic enough for student scope); separate from environment if animated later.

**Pipeline**

- **Format** — 3D models and textures for Unity 6 / URP; naming and folder layout under `Assets/` per team convention.
- **LOD / performance** — single primary scene; keep draw calls and texture memory reasonable for class machines.

