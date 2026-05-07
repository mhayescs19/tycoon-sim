# Validation — Computer Dashboard UI

## Automated

- Unity compiles with no new errors after dashboard UI changes
- EditMode tests pass for dashboard defaults:
  - Exactly four cards are present (`Vibe Code`, `Bug Bounty`, `OpenClaw`, `Airbed`)
  - `OpenClaw` and `Airbed` initialize as `Locked` with disabled "Purchase Hustle" state
  - `Vibe Code` initializes as `Active` and maps to `OpenVibeCode` action
- PlayMode/UI tests pass:
  - Keyboard/controller focus moves through dashboard controls in intended order
  - Locked cards are not invokable
  - Invoking `Vibe Code` still routes to existing implemented vibe code interface
  - Placeholder cards do not change `GameManager` balance or ownership state

## Manual Walkthrough

1. Enter Play mode and approach/interact with Gary's computer  
2. Confirm dashboard opens with four cards: `Vibe Code`, `Bug Bounty`, `OpenClaw`, `Airbed`  
3. Confirm `OpenClaw` and `Airbed` cards are visibly gray and show "Purchase Hustle" locked state  
4. Confirm `Vibe Code` card opens the already-implemented vibe code interface  
5. Confirm non-vibe cards currently perform no gameplay action (UI placeholder behavior only)  
6. Navigate dashboard using keyboard/controller only and verify focus is visible and predictable  
7. Close and reopen dashboard; confirm focus and visual states still behave correctly  

## Edge Cases

- Dashboard opened repeatedly in the same session does not duplicate card entries
- Focus does not get trapped on disabled locked cards
- Opening dashboard while already in vibe code flow does not create duplicate overlays
- Locked card styling remains readable in low-light scene conditions

## Tone Check

- Card copy is concise, product-like, and matches the SF startup builder tone
- Locked states communicate "not yet purchasable" without sounding like an error
- UI remains clean and functional over highly polished; this is a vertical-slice pass

## Definition of Done

- [ ] Computer dashboard opens from existing computer interaction
- [ ] Four hustle cards are visible with correct names
- [ ] `OpenClaw` and `Airbed` render as grayed locked "Purchase Hustle" cards/buttons
- [ ] `Vibe Code` card still triggers existing implementation
- [ ] Other cards are placeholders with no backend hustle logic
- [ ] Keyboard/controller navigation works end-to-end
- [ ] No regressions in existing camera/HUD/computer flow
- [ ] Automated checks above pass
