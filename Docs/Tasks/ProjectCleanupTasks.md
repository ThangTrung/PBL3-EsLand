# Tasks: Project Cleanup & Sync

## Phase 1: Player Prefab Maintenance
- [ ] Task: Sync `EquipmentManager` to Player Prefab.
  - Acceptance: `Assets/Prefabs/Character/Player/Pawn_black.prefab` contains `Gameplay.Equipment.EquipmentManager`.
  - Verify: `manage_asset` get_components check.
  - Files: `Assets/Prefabs/Character/Player/Pawn_black.prefab`

## Phase 2: Utility Script Enhancement
- [ ] Task: Upgrade `ResourceSetupUtility.cs` logic.
  - Acceptance: Script handles Rocks (10 HP, no animator check) and Trees (3 HP, animator check), sets layer to 12.
  - Verify: Code inspection for "Rock" keyword and Layer 12 assignment.
  - Files: `Assets/Scripts/Editor/ResourceSetupUtility.cs`

## Phase 3: Global Scene Synchronization
- [ ] Task: Execute Cleanup Utility.
  - Acceptance: All "Tiny_tree" and "Rock" objects in the scene are on Layer 12 and have necessary scripts.
  - Verify: Console log output and manual check of a Rock object via MCP.
  - Files: N/A (Editor Action)
