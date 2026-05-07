# Plan — Computer Dashboard UI

## 1. Dashboard Structure and Data

1.1 Create a dashboard UI controller script under `Assets/Scripts/` for card rendering and button dispatch  
1.2 Define local card configuration for `Vibe Code`, `Bug Bounty`, `OpenClaw`, and `Airbed`  
1.3 Add explicit `CardState` (`Active`/`Locked`) and action type (`OpenVibeCode`/`Placeholder`) fields  
1.4 Set default states: `Vibe Code` + `Bug Bounty` active, `OpenClaw` + `Airbed` locked  

## 2. Dashboard Panel Layout

2.1 Build/extend the computer dashboard panel in UGUI (screen-space overlay)  
2.2 Add reusable card prefab/layout blocks with title, subtitle, cost label, and action button  
2.3 Place four cards in a consistent grid/stack layout sized for keyboard/controller traversal  
2.4 Add locked styling variant (gray background, dim text/icon, disabled button visual)  

## 3. Card Interaction Wiring

3.1 Wire `Vibe Code` card action to the existing implemented vibe code interface  
3.2 Wire `Bug Bounty`, `OpenClaw`, and `Airbed` actions to placeholder handlers (no economy mutation)  
3.3 Ensure locked cards show "Purchase Hustle" label and remain non-interactive in this slice  
3.4 Keep all interactions inside computer dashboard flow (no corkboard interaction path)  

## 4. Navigation and UX States

4.1 Configure deterministic focus order across cards/buttons for keyboard/controller  
4.2 Add selected/hover/focused visual states that remain readable in dim lighting  
4.3 Ensure dashboard can be opened/closed without breaking focus restoration  
4.4 Verify no dead-end navigation paths and no invisible focused elements  

## 5. Scene Integration

5.1 Connect dashboard panel to existing computer interaction entrypoint  
5.2 Confirm existing vibe code implementation still opens correctly from dashboard route  
5.3 Confirm non-vibe cards do not trigger gameplay systems yet  
5.4 Validate no regressions in current HUD/camera behavior while dashboard is active  

## 6. Tests

6.1 Add EditMode tests for card configuration defaults (labels, state, action type)  
6.2 Add PlayMode/UI tests for locked card disabled state and focus traversal order  
6.3 Add PlayMode/UI test asserting `Vibe Code` button invokes existing interface path  
6.4 Add regression test/assertion that placeholder cards do not mutate money or hustle ownership  
