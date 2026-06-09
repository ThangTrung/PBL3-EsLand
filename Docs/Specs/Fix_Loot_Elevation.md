# Spec: Fix Loot Elevation Mismatch

## Objective
Fix the issue where items dropped from enemies or resources always appear on 'Elevation_A' even when the source entity is on Elevation B or C. Ensure dropped items correctly inherit and maintain the elevation state of their source.

## Root Cause Analysis
- **Physical vs. Logical Elevation:** The `LootSpawner` assumes `transform.parent` is the elevation container. However, enemies and arena bosses are often parented to root objects like `Boss_Arenas` or `EnemyFactory`.
- **Static Inheritance:** Items only look at their parent's `AutoAssignSortingLayer` at `Start()`. If the parent is a root object without an elevation layer, they default to Elevation A.
- **Dynamic State Ignored:** Entities use `ElevationAgent` to change their layer at runtime, but this state was not being passed to the spawned loot.

## Solution Architecture
1. **Elevation Context Propagation:** Modify the Loot System to accept an explicit `elevationLayer` string.
2. **Dynamic Update:** The `LootFactory` will manually call `ElevationAgent.ChangeElevation()` on spawned items using the provided context.
3. **Source-Linked Spawning:** `LootSpawner` will query its own `ElevationAgent` to get the current layer before triggering a drop.

## Tech Stack
- Unity 2D
- C# (SOLID)

## Project Structure
- `Assets/Scripts/Core/Contracts/World/ILootFactory.cs` (Refactor)
- `Assets/Scripts/Gameplay/World/LootFactory.cs` (Refactor)
- `Assets/Scripts/Gameplay/World/LootSpawner.cs` (Refactor)

## Success Criteria
- Killing a Boss on Elevation C (Tầng C) results in loot appearing on Elevation C.
- Chopping a tree on Elevation B results in wood appearing on Elevation B.
- Staggered loot drops (bulk) correctly inherit the elevation for every item in the batch.

## Boundaries
- **Always do:** Check for the presence of `ElevationAgent` before calling methods on it.
- **Ask first:** If we need to change how `ElevationAgent` stores its state.
- **Never do:** Hardcode "Elevation_A", "Elevation_B", etc. Always use variables from the agent.
