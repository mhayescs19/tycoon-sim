# Plan — Bug Bounty Mechanic

## 1. Bounty data + sequencing core

1.1 Create bounty data model and runtime status model (`Active`, `ClaimedInRoom`, `Succeeded`, `Failed`)  
1.2 Add the 100-bounty catalog as structured data (ScriptableObject or JSON/text asset under `Assets/`)  
1.3 Implement sequential selection manager:
- maintain current bounty index cursor
- select next bounty from list order
- increment cursor and wrap to `0` at end  
1.4 Add reward-range-to-tier resolution (`$10..$500` -> Tier 1..5)  
1.5 Add per-attempt penalty percent roll based on tier bands (`5%..15%`)

## 2. Computer dashboard bug bounty lists

2.1 Add Bug Bounty section to computer interface with three panes:
- Active
- Successful
- Failed  
2.2 Active list entries show:
- title
- reward
- instruction: "Claim in room at wall note"  
2.3 Successful list entries show:
- bounty title
- earned amount
- completion timestamp
- running total earned  
2.4 Failed list entries show:
- bounty title
- penalty amount
- failure timestamp
- running total penalties  
2.5 Ensure keyboard/controller navigation parity in all bounty list UI controls

## 3. Wall-note spawn, cadence, and claim flow

3.1 Build wall-note spawner that instantiates active bounty notes at defined wall anchors  
3.2 Add spawn scheduler with rolled interval per spawn (`15..45` seconds)  
3.3 Add "new bug bounty spawned" alert/notification in UI  
3.4 Add proximity interaction to claim/start bounty from note  
3.5 Prevent duplicated claim on same note while already in progress  
3.6 On claim, transition bounty state to `ClaimedInRoom` and open puzzle overlay  
3.7 On completion/failure, despawn/refresh note and advance to next bounty in sequence

## 4. Generator puzzle mechanic (DBD-style)

4.1 Implement puzzle session controller with:
- progress meter
- random skill-check trigger timing
- success/great/fail windows
- miss/fail conditions  
4.2 Difficulty scaling by tier:
- number of checks
- indicator speed
- success window size
- allowed misses/time budget  
4.3 Success path:
- mark bounty `Succeeded`
- award full reward dollars via `GameManager`
- write history row and totals  
4.4 Failure path:
- mark bounty `Failed`
- compute penalty using rolled tier percentage
- deduct via `GameManager` with floor/clamp behavior
- write history row and totals

## 5. Economy integration + telemetry/debug

5.1 Add explicit API methods for bug bounty reward/penalty mutations (or dedicated service wrapper over `GameManager`)  
5.2 Add debug logs and optional dev-only overlay values for:
- current bounty index
- in-progress bounty
- rolled penalty percent/value
- current spawn interval and next spawn ETA  
5.3 Ensure LOC economy loop remains untouched and compatible

## 6. Tests and balancing pass

6.1 EditMode tests:
- sequential cursor advance + wrap behavior
- reward-to-tier mapping
- penalty percent always within correct tier band
- success/failure accounting totals  
6.2 PlayMode tests:
- note spawn cadence (`15..45s`) + alert visibility
- note spawn/claim lifecycle
- puzzle resolution path writes correct history bucket  
6.3 Manual balancing pass (Balanced target):
- verify low/mid/high bounty feel
- verify risk feels meaningful but not punishing
- verify UI readability for history/totals
