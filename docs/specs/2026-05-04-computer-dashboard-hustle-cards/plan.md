# Plan — Computer Dashboard Hustle Cards

## 1. Dashboard layout shell

1.1 Identify current computer dashboard prefab/panel used by Vibe Code flow  
1.2 Add/adjust a dedicated Hustles section in the same dashboard container  
1.3 Define card slots/order: Vibe Code, Bug Bounty, OpenClaw, Airbed  
1.4 Keep spacing/labels readable with minimal-polish styling (vertical slice target)

## 2. Card state presentation

2.1 Implement active card visuals for Vibe Code and Bug Bounty  
2.2 Implement locked gray "Purchase Hustle" visuals for OpenClaw and Airbed  
2.3 Standardize per-card text areas (name, short effect/cost placeholder, state CTA)  
2.4 Ensure locked cards are visually distinct even when focused

## 3. Interaction and navigation polish

3.1 Wire keyboard focus order across dashboard controls and hustle cards  
3.2 Wire controller focus order using current Input System UI navigation  
3.3 Add explicit focused/selected visual state for all interactive elements  
3.4 Verify navigation works without mouse input (keyboard-only and controller-only passes)

## 4. Vibe Code integration boundary

4.1 Keep existing Vibe Code button behavior unchanged  
4.2 Keep Bug Bounty/OpenClaw/Airbed buttons as no-op placeholder interactions in this phase  
4.3 Add clear TODO markers/comments for follow-up hustle logic ticket

## 5. Test and QA coverage

5.1 Add/extend UI-focused tests where practical (focus traversal/order assertions)  
5.2 Manual QA pass for dashboard open/close + card visibility + focus state transitions  
5.3 Validate no regressions to existing Vibe Code interaction path
