# Validation — Computer Dashboard Hustle Cards

## Automated

- Unity project opens with no compile errors in Console
- Unity Test Runner command set passes for existing suite:
  - EditMode tests pass
  - PlayMode tests pass (if present in repository)
- Any new UI navigation tests added for this feature pass, including:
  - Focus order reaches all four hustle cards
  - Locked cards still receive focus-visible states
  - Vibe Code card remains actionable through existing path

## Manual walkthrough

1. Open `SampleScene` and enter Play mode
2. Interact with the computer to open the dashboard
3. Confirm dashboard shows cards in expected set/order:
   - Vibe Code
   - Bug Bounty
   - OpenClaw
   - Airbed
4. Confirm visual states:
   - Vibe Code and Bug Bounty appear active
   - OpenClaw and Airbed appear gray locked "Purchase Hustle"
5. Using keyboard only, traverse focus across all dashboard controls and cards
6. Using controller only, traverse focus across all dashboard controls and cards
7. Activate Vibe Code card and verify existing implemented behavior still works
8. Activate Bug Bounty/OpenClaw/Airbed and verify they remain placeholder/no-op in this phase
9. Close dashboard and reopen to confirm consistent card states and navigation behavior

## Edge cases

- Focus starts outside expected element when dashboard opens
- Focus gets trapped on a locked card or skips locked cards unexpectedly
- Controller and keyboard focus order diverge from one another
- Locked gray styling becomes unreadable when focused/selected

## Definition of done

- [ ] Computer dashboard presents all four hustle cards
- [ ] Vibe Code + Bug Bounty cards render as active
- [ ] OpenClaw + Airbed render as gray locked "Purchase Hustle" cards
- [ ] Keyboard navigation is complete and visually clear
- [ ] Controller navigation is complete and visually clear
- [ ] Existing Vibe Code behavior remains functional
- [ ] Non-Vibe hustle buttons remain UI-only placeholders in this phase
- [ ] No compile errors; automated tests pass
