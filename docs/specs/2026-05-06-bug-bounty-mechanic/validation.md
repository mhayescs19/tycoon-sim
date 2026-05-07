# Validation — Bug Bounty Mechanic

## Automated

- Unity project opens cleanly (no compile errors in Console).
- EditMode tests pass for:
  - 100-item bounty catalog integrity (unique IDs, reward range `$10..$500`)
  - reward-to-difficulty tier mapping
  - sequential cursor/index advances one-by-one and wraps at list end
  - spawn interval roll is always in `15..45` seconds
  - penalty roll range by tier:
    - Tier 1: `5%..7%`
    - Tier 2: `7%..9%`
    - Tier 3: `9%..11%`
    - Tier 4: `11%..13%`
    - Tier 5: `13%..15%`
  - success accounting (`+reward`) and failure accounting (`-penalty`)
- PlayMode tests pass for:
  - new bounty spawns at cadence within `15..45` seconds
  - "new bug bounty spawned" alert appears on spawn
  - active bounty note spawn in room
  - claim interaction opens puzzle
  - puzzle success moves bounty to Successful list and updates totals
  - puzzle failure moves bounty to Failed list and updates totals

## Manual walkthrough

1. Open `SampleScene` and enter Play mode.
2. Open computer dashboard and navigate to Bug Bounty section.
3. Verify Active list is populated and each item includes claim instruction for room wall notes.
4. Walk to wall and interact with a spawned bounty note.
5. Confirm generator puzzle opens with tier-appropriate difficulty.
6. Complete one bounty successfully:
   - payout equals full bounty amount
   - entry appears in Successful list with amount and timestamp
   - successful total increases correctly
7. Fail one bounty attempt:
   - penalty equals rolled percentage (`5%..15%` by tier) of bounty value
   - entry appears in Failed list with penalty and timestamp
   - failed total increases correctly
8. Wait through multiple spawn windows and verify each spawn happens in `15..45` second interval.
9. Verify each spawn shows a "new bug bounty spawned" alert.
10. In a debug/accelerated run, verify bounty selection advances in list order and wraps after `BB-100`.

## Edge cases

- Player leaves interaction range mid-puzzle.
- Player closes UI during a skill check.
- Duplicate claim attempts on same active note.
- Balance near zero when penalty applies (should clamp/fail safely per economy rules).
- Controller-only and keyboard-only navigation in Bug Bounty dashboard panes.
- Spawn timer overlap when player is mid-puzzle.

## Tone check

- Active-list instruction text is clear and concise ("Claim in room at wall note").
- Success/failure copy avoids ambiguity (earned vs penalty).
- Risk messaging fits balanced tone (not trivial, not punitive).

## Definition of done

- [ ] Computer interface shows Active, Successful, and Failed bug bounty lists.
- [ ] Successful list includes per-bounty earned amount and cumulative total.
- [ ] Failed list includes per-bounty penalty amount and cumulative total.
- [ ] 100-bounty catalog is implemented and used for sequential cycling.
- [ ] Bounty selection advances one-by-one and wraps to start after end.
- [ ] New bounties spawn every `15..45` seconds.
- [ ] New-spawn alert is shown when each bounty appears.
- [ ] Wall notes spawn for active bounties and can be claimed in-room.
- [ ] Generator puzzle scales with bounty difficulty/value.
- [ ] Success awards full bounty reward.
- [ ] Failure penalty is percentage-based (`5%..15%`), not full bounty amount.
- [ ] Automated EditMode/PlayMode checks pass.
