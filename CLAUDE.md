# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**Gary Tan Tycoon Simulator** — a 3D Unity tycoon game. Player types to generate Lines of Code (LOC) → earns Dollars → buys Hustles → compounds income. Single 3D apartment scene. Optional slots room down the hall (Apartment 168). See `docs/project-spec.md` for the full design.

## Unity Setup

- **Unity version:** 6000.3.6f1 (Unity 6.3)
- **Render pipeline:** URP 17.3.0
- **Input:** New Input System (`com.unity.inputsystem` 1.18.0) — use `InputSystem_Actions.inputactions` for bindings
- **Scene:** `Assets/Scenes/SampleScene.unity` — the only scene, contains the full apartment

## Key Packages

| Package | Version | Purpose |
|---------|---------|---------|
| URP | 17.3.0 | Rendering |
| Input System | 1.18.0 | All player input |
| AI Navigation | 2.0.9 | NavMesh if needed |
| Test Framework | 1.6.0 | Unity Test Runner |

## Architecture

All custom scripts belong under `Assets/Scripts/`. Follow this intended structure as the project grows:

- **`GameManager.cs`** — singleton owning LOC count, LOC/sec, Dollar balance, and conversion rate. Single source of truth for all economy values.
- **`TypingInput.cs`** — listens to any keypress via the Input System and calls `GameManager.AddLOC()`.
- **`HustleManager.cs`** — tracks purchased Hustles, fires passive income ticks, and tells the apartment to spawn objects.
- **`SlotMachine.cs`** — wager/spin/resolve logic; reads and writes Dollars directly through `GameManager`.
- **`HUD.cs`** — subscribes to `GameManager` events and updates LOC count and LOC/sec display.

### Economy constants (per spec)
- LOC → Dollars at a fixed rate (define in `GameManager`)
- Hustles are one-time purchases; passive income runs on a fixed tick interval
- OpenClaw is the only Hustle that also generates passive LOC/sec

## Team & Ownership

| Name | Role | Tickets |
|------|------|---------|
| Michael Hayes | Level Design | Base map, vibe coding computer logic |
| Matt McGuire | Level Design/Mechanics | Slot machine logic |
| Kaylee Lace | Art/Animation | Gary's apartment assets |

## Running Tests

Open Unity → **Window → General → Test Runner** → run EditMode or PlayMode tests.
