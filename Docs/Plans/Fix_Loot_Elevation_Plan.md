# Plan: Fix Loot Elevation Mismatch

## Phase 1: Context Propagation (SOLID Refactoring)
1. **Update `ILootFactory`**:
    - Add `string elevationLayer` parameter to `SpawnLoot`.
    - Add `string elevationLayer` parameter to `SpawnLootBulk`.
2. **Update `LootFactory`**:
    - In `SpawnLoot`, if `elevationLayer` is provided, call `ElevationAgent.ChangeElevation()` on the spawned object.
    - Update `SpawnStaggeredRoutine` and `DropSingle` to handle the elevation string.

## Phase 2: Source Synchronization
1. **Update `LootSpawner`**:
    - In `Awake`, cache a reference to the local `ElevationAgent`.
    - In `SpawnLoot`, retrieve the current elevation from the agent (or fallback to parent's layer if missing).
    - Pass this elevation string to `LootFactory.Instance.SpawnLootBulk`.

## Phase 3: Validation
1. **Manual Check (Ogre Boss)**: Kill Ogre Boss on Elevation A. Check layer.
2. **Manual Check (Thief Boss)**: Kill Thief Boss on Elevation C. Check layer.
3. **Hierarchy Check**: Verify that `LootFactory` correctly updates the `SortingLayer` via the `AutoAssignSortingLayer` script.

## Tasks
- [ ] Task 1: Update `ILootFactory` interface.
- [ ] Task 2: Implement dynamic elevation in `LootFactory`.
- [ ] Task 3: Update `LootSpawner` to detect and pass local elevation.
- [ ] Task 4: Verify results with Thief Boss.
