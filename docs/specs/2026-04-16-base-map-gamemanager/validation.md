# Validation — Base Map + GameManager

## Automated

- Unity Test Runner (EditMode) passes all tests in `GameManagerTests.cs`:
  - `AddLOC` increments `LOCCount` correctly
  - `AddLOC` converts to Dollars at the expected rate (10 LOC = $1)
  - `SpendDollars` reduces balance and clamps at 0
  - `OnBalanceChanged` event fires on every balance mutation
- No compiler errors or warnings in the Unity console on project open

## Manual Walkthrough

1. Open `SampleScene` — scene loads without errors
2. Enter Play mode — `GameManager` singleton initializes; console shows no errors
3. HUD displays `LOC: 0 (0.0/sec)` and `$ 47.00` on screen
4. In Play mode, call `GameManager.Instance.AddLOC(10)` via a test button or console — verify `$ 48.00` appears and LOC count updates
5. Call `GameManager.Instance.SpendDollars(100)` — verify balance floors at `$0.00`, not negative

## Edge Cases

- `SpendDollars` with amount greater than balance → clamps to 0, fires event with 0
- `AddLOC(0)` → no-op, no event fired
- Duplicate `GameManager` in scene → second instance destroys itself (singleton guard)

## Definition of Done

- [ ] Room geometry visible in scene (floor + 4 walls + ceiling, no props)
- [ ] `GameManager` singleton in Hierarchy with correct initial values in Inspector
- [ ] `OnBalanceChanged` and `OnLOCChanged` events implemented
- [ ] Stub HUD shows live LOC count and Dollar balance
- [ ] All 4 EditMode tests pass
- [ ] No console errors in Play mode
