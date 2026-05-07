# Requirements — Bug Bounty Mechanic

## Scope

**Included:**
- Computer dashboard Bug Bounty interface with three sections:
  - Active bounties (includes instruction to claim in-room on wall notes)
  - Successful bounties (per-bounty earned amount + running total earned)
  - Failed bounties (per-bounty penalty amount + running total penalties)
- Bug bounty pool of 100 authored bounties cycled in fixed order (MVP).
- Sequential bounty selection using a cursor/index that advances one-by-one and loops at end of list.
- In-room wall bounty note spawning for active bounties.
- Spawn cadence system: new bug bounty spawns every **15 to 45 seconds**.
- Player alert when a new bug bounty spawns.
- Dead-by-Daylight-style generator skill-check puzzle flow for bounty attempts.
- Difficulty tied to bounty value (`$10` to `$500`).
- Economy outcomes:
  - Success: award full bounty amount.
  - Failure: apply penalty of **5% to 15%** of bounty value (varies by difficulty/value tier).

**Not included:**
- Visual final art/VFX polish for wall notes or puzzle widgets.
- Multiplayer/networked bounty competition.
- New dependencies/packages outside existing Unity stack.

### Recommended ship strategy (single feature, phased implementation)

This is one full feature spec, but should be implemented in slices to reduce integration risk:
1. Data + sequencing + computer lists
2. Wall-note spawning/claim flow
3. Puzzle gameplay + outcomes + balancing
4. QA tuning and polish

## Data model

| Field | Type | Notes |
|------|------|------|
| `BountyId` | `string` | Unique key, e.g. `BB-001` |
| `Title` | `string` | Short bug bounty prompt |
| `Description` | `string` | Optional 1-line context |
| `RewardDollars` | `int` | Integer in `$10..$500` |
| `DifficultyTier` | `int` | `1..5`, derived from reward band |
| `PenaltyPercent` | `float` | Randomized per attempt in tier range |
| `Status` | `enum` | `Active`, `ClaimedInRoom`, `Succeeded`, `Failed` |
| `SpawnedNoteId` | `string` | Wall note link for active bounty |
| `CompletedAtUtc` | `string` | Timestamp for history list |
| `SpawnIntervalSec` | `float` | Rolled per spawn in `15..45` |
| `NextSpawnAtUtc` | `string` | Scheduler timestamp for next bounty spawn |

### Reward -> difficulty mapping

| Tier | Reward range | Puzzle profile | Penalty % range |
|------|--------------|----------------|-----------------|
| 1 | `$10..$50` | Easy | `5%..7%` |
| 2 | `$51..$120` | Medium-easy | `7%..9%` |
| 3 | `$121..$220` | Medium | `9%..11%` |
| 4 | `$221..$350` | Hard | `11%..13%` |
| 5 | `$351..$500` | Expert | `13%..15%` |

### Generator puzzle behavior (DBD-style)

- Puzzle is a timed progress interaction with random skill-check triggers.
- Each skill check includes a moving indicator and success window (optional smaller "great" window).
- Required check count, speed, and success-window size scale by `DifficultyTier`.
- Failing an attempt (miss/time-out/too many misses) resolves bounty as failed.
- Completing required checks resolves bounty as success.

## Decisions

- **MVP selection strategy:** Use simple sequential cycling through the 100-item list (index cursor with wraparound).
- **Balanced tuning target:** Moderate challenge and moderate risk profile.
- **Difficulty linked to payout:** Higher reward bounties produce harder puzzle parameters.
- **Penalty model:** Failure penalty is percentage-based (`5%..15%`) by tier, not full bounty loss.
- **UI accounting clarity:** Successful and failed history both show per-bounty and cumulative totals.
- **Room claim requirement:** Active bounties must be claimed in-room from wall notes, not completed from dashboard alone.
- **Spawn cadence:** Each new bounty spawn rolls a delay in `15..45` seconds.
- **Spawn communication:** Show a clear "new bug bounty spawned" alert when a bounty appears.

## Context

- Aligns with `docs/specs/project-spec.md` Bug Bounty direction:
  - active wall notes
  - generator-style skill check
  - computer dashboard management
- Respects `docs/specs/mission.md` interaction model:
  - screen-space UI overlays for interaction
  - no camera cut/zoom behavior added
- Respects `docs/specs/tech-stack.md`:
  - Unity 6.3 + URP + Input System
  - no new packages

## Bug bounty catalog (100 items)

Each item below is eligible for sequential cycling in list order (cursor advances and wraps).

| ID | Title | Reward |
|----|-------|--------|
| BB-001 | NullRef in settings modal | $15 |
| BB-002 | Save button double-submit | $20 |
| BB-003 | Tooltip overflow on low res | $25 |
| BB-004 | Broken tab order in auth form | $30 |
| BB-005 | Ghost click on modal close | $35 |
| BB-006 | Incorrect loading spinner state | $40 |
| BB-007 | Missing icon fallback | $45 |
| BB-008 | Toast appears behind panel | $50 |
| BB-009 | Profile image crop mismatch | $18 |
| BB-010 | Stale cache banner persists | $22 |
| BB-011 | Off-by-one in pagination | $55 |
| BB-012 | Search clears filters unexpectedly | $60 |
| BB-013 | Retry action misses last request | $65 |
| BB-014 | Session timer drift issue | $70 |
| BB-015 | Keyboard shortcut conflict | $75 |
| BB-016 | Notification dedupe bug | $80 |
| BB-017 | Form validation race condition | $85 |
| BB-018 | Scroll reset after refresh | $90 |
| BB-019 | Undo stack loses one step | $100 |
| BB-020 | Filter chip removal desync | $110 |
| BB-021 | Permission badge stale after role change | $120 |
| BB-022 | Export csv truncates unicode | $130 |
| BB-023 | Date parser timezone mismatch | $140 |
| BB-024 | Audit row sorting unstable | $150 |
| BB-025 | Draft autosave deadlock | $160 |
| BB-026 | Webhook retry jitter bug | $170 |
| BB-027 | Queue item duplication | $180 |
| BB-028 | Background job status phantom complete | $190 |
| BB-029 | Reconnect logic misses first packet | $200 |
| BB-030 | Billing preview rounds wrong | $210 |
| BB-031 | Leaderboard rank tie-break wrong | $220 |
| BB-032 | Chat mention parser false positives | $58 |
| BB-033 | Sticky header overlaps dropdown | $62 |
| BB-034 | Attachment preview orientation bug | $68 |
| BB-035 | Mobile nav flicker | $74 |
| BB-036 | Focus trap escapes modal | $82 |
| BB-037 | Theme switch one-frame flash | $88 |
| BB-038 | Keyboard repeat floods action | $96 |
| BB-039 | Replay timeline marker drift | $104 |
| BB-040 | Chart tooltip stale point | $112 |
| BB-041 | AB flag mismatch in edge region | $125 |
| BB-042 | Websocket backoff reset bug | $135 |
| BB-043 | Report generator memory leak | $145 |
| BB-044 | Parser ignores escaped delimiter | $155 |
| BB-045 | Sync conflict picker wrong default | $165 |
| BB-046 | Merge resolver drops blank lines | $175 |
| BB-047 | Partial update overwrites null | $185 |
| BB-048 | Queue priority inversion | $195 |
| BB-049 | Batch action timeout regression | $205 |
| BB-050 | Tenant boundary cache bleed | $220 |
| BB-051 | Inventory count eventually wrong | $230 |
| BB-052 | Refund idempotency edge case | $240 |
| BB-053 | Retry budget not respected | $250 |
| BB-054 | Circuit breaker half-open flaps | $260 |
| BB-055 | Search index stale shard | $270 |
| BB-056 | Leader election split-brain window | $280 |
| BB-057 | Snapshot restore ordering bug | $290 |
| BB-058 | Event replay duplicates side-effect | $300 |
| BB-059 | Rule engine precedence mismatch | $315 |
| BB-060 | Delayed job orphan handling | $330 |
| BB-061 | Multi-region failover warmup bug | $345 |
| BB-062 | Dependency graph cycle false negative | $350 |
| BB-063 | CSS grid overflow in RTL | $28 |
| BB-064 | Copy to clipboard strips newline | $34 |
| BB-065 | Markdown code block language loss | $42 |
| BB-066 | Sidebar collapse state not persisted | $48 |
| BB-067 | Input debounce too aggressive | $57 |
| BB-068 | Filename sanitizer misses emoji | $66 |
| BB-069 | Token refresh duplicate request | $76 |
| BB-070 | Avatar color hash inconsistency | $86 |
| BB-071 | Presence indicator stale after sleep | $98 |
| BB-072 | Draft recover banner loops | $108 |
| BB-073 | Feature gate ignores org override | $118 |
| BB-074 | Currency formatter locale fallback | $128 |
| BB-075 | Command palette score ranking bug | $138 |
| BB-076 | Query planner wrong join path | $225 |
| BB-077 | Deadletter queue redrive mismatch | $245 |
| BB-078 | Priority scheduler starvation | $265 |
| BB-079 | Segment compaction corruption guard | $285 |
| BB-080 | Replica lag alarm false positive | $305 |
| BB-081 | Metrics cardinality explosion path | $325 |
| BB-082 | Lock contention hotspot regression | $340 |
| BB-083 | Schema migration rollback fault | $360 |
| BB-084 | Distributed trace parent loss | $375 |
| BB-085 | Cache stampede on cold key | $390 |
| BB-086 | Cross-shard transaction leak | $405 |
| BB-087 | Message ordering guarantee breach | $420 |
| BB-088 | Blob compactor partial write | $435 |
| BB-089 | Sandbox escape in parser edge | $450 |
| BB-090 | Replay attack nonce reuse | $465 |
| BB-091 | Checkpoint restore data skew | $480 |
| BB-092 | Quorum write stale-read window | $495 |
| BB-093 | Wallet reconciliation drift | $500 |
| BB-094 | Geo routing asymmetry fault | $355 |
| BB-095 | Burst load admission collapse | $370 |
| BB-096 | Async cancellation token leak | $385 |
| BB-097 | Rollup aggregator double-count | $410 |
| BB-098 | Secret rotation race | $440 |
| BB-099 | Compensating transaction miss | $470 |
| BB-100 | Cross-tenant ACL bypass edge | $490 |
