# Validation — Vibe Coding Computer

## Automated

- EditMode tests in `ComboMultiplierTests.cs` all pass:
  - Multiplier initializes at 1×
  - Rapid keypresses increment multiplier toward 3×
  - Decay resets multiplier to 1× after `decayDelay`
  - `GameManager.LOCPerSec` equals `baseLocPerSec * multiplier`
- No compiler errors on project open

## Manual Walkthrough

1. Enter Play mode — computer display panel is **not visible**
2. Pan camera pivot toward the desk (within ~2.5m) — display panel appears with dark background and cursor
3. Press any key — GStack source code characters appear on screen (3 chars per keypress), scrolling like an IDE
4. Type quickly for 3+ seconds — HUD `LOC/sec` value climbs and multiplier indicator reflects 2×–3×
5. Stop typing for 1.5s — `LOC/sec` decays back toward base rate
6. Pan camera away from desk — display panel disappears, `LOC/sec` drops to 0
7. Verify code feed wraps: type until end of corpus — display continues from the top without error

## Edge Cases

- Keypress outside proximity range → no LOC added, no display change
- Corpus file missing from Resources → log error, display shows placeholder text "// loading..."
- Player holds a key (key repeat) → each repeat event counts as a keypress (intentional — holding counts as fast typing)

## Tone Check

- Code displayed must be real GStack source — not Lorem Ipsum or random characters
- Dark panel feel: near-black background (#1a1a1a), white/light-grey monospace text — matches the "dim SF apartment" aesthetic

## Definition of Done

- [ ] `GStackSourceCode.txt` in `Assets/Resources/`
- [ ] Desk + monitor placeholder visible in scene
- [ ] Proximity trigger activates/deactivates display correctly
- [ ] Any keypress near desk advances code feed and awards LOC
- [ ] Combo multiplier visibly affects LOC/sec in HUD
- [ ] Display scrolls upward as new lines appear
- [ ] All ComboMultiplier EditMode tests pass
