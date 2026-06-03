# Implementation Plan: Elevation Transition System

## Overview
This system allows Player and Enemy entities to seamlessly switch between different map elevations (Elevation_A, B, C, D) when using stairs. It ensures that the `Sorting Layer` and `Physics Interaction` are updated automatically to match the current floor, preventing rendering artifacts.

## Architecture Decisions
- **Agent-Gateway Pattern:** Characters carry an `ElevationAgent` (the listener), and stairs have `ElevationGateway` (the trigger).
- **Decoupling:** The Agent doesn't care *how* the trigger works; it just receives the "Switch to Layer X" command.
- **Y-Sorting Integration:** Reuses the existing `AutoAssignSortingLayer` logic to refresh visuals instantly.

## Task List

### Phase 1: Infrastructure (Scripts)
- [ ] **Task 1: Create ElevationAgent.cs**
  - Acceptance: Script exists in `Gameplay/Environment`, requires `AutoAssignSortingLayer`, and has `ChangeElevation` method.
  - Verify: Compilation success.
- [ ] **Task 2: Create ElevationGateway.cs**
  - Acceptance: Script exists, handles `OnTriggerEnter2D`, and calls `agent.ChangeElevation`.
  - Verify: Compilation success.

### Phase 2: Prefab Integration
- [ ] **Task 3: Update Player Prefabs**
  - Acceptance: All 5 Player prefabs (`Pawn_black` to `yellow`) have the `ElevationAgent` component attached to Root.
  - Verify: Check components via MCP on prefabs.
- [ ] **Task 4: Create Gateway Prefab**
  - Acceptance: A reusable prefab `Elevation_Gateway.prefab` with a Trigger Collider and the script.
  - Verify: Prefab exists in `Assets/Prefabs/Environment`.

### Phase 3: Scene Configuration (Manual + Guided)
- [ ] **Task 5: Configure Map2Kt Stairs**
  - Acceptance: Stairways in `Map2Kt` have top and bottom gateways.
  - Verify: User confirms character switches layers correctly when walking up/down.

## Risks and Mitigations
| Risk | Impact | Mitigation |
|------|--------|------------|
| Player gets stuck between layers | Medium | Gateways should be thick enough to ensure trigger is hit. |
| Enemy AI doesn't trigger | Low | Ensure Enemy prefabs also get the Agent component. |
| Rendering flicker during switch | Low | `ApplyLayer()` is fast, but we can add a tiny delay if needed. |

## Open Questions
- Do you want the **Physics Layer** (Layer 6/7) to also change based on elevation? (e.g., prevent someone on Tầng A from hitting someone on Tầng B).
- Should I apply the `ElevationAgent` to specific Enemy prefabs now?
