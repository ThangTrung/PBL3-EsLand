# Plan: Refactoring Loot System and Fixing Boss Drops

## Phase 1: Infrastructure & SOLID Refactoring
1. **Create `ILootFactory`**: Interface for spawning loot items using Object Pooling.
2. **Implement `LootFactory`**: 
    - Use `ObjectPoolManager.Instance.Get` for `GenericPickup`.
    - Setup `ItemPickup` component and `SpriteRenderer`.
3. **Refactor `LootSpawner`**:
    - Remove `Instantiate` calls.
    - Inject or use `LootFactory`.
    - Implement a `StaggeredDrop` logic (optional but recommended for high counts).

## Phase 2: Physics & Distribution Optimization
1. **Spread Logic**: Modify `DropItem` to spawn items with a small random offset *before* applying force.
2. **Physics Layer**: Ensure `GenericPickup` is on a layer that doesn't collide with other loot items (optional, depending on desired feel) to prevent "explosions".
3. **Bulk Drop Aggregation**: If an item count is > 20, spawn "Stacks" (represented by a single object with multiple quantity) instead of individual objects.

## Phase 3: Boss Specific Adjustments
1. **Animation Events**: Add an animation event to the Ogre Boss death animation to trigger `TriggerLootDrop` at a specific visual moment (e.g., when he hits the ground).
2. **Lifecycle Sync**: Ensure the Boss remains in the scene (even if invisible/dead) long enough for all loot to finish spawning if staggered.

## Phase 4: Validation
1. **Kill Ogre Boss**: Confirm items are visible and interactable.
2. **Check Console**: Ensure no `null` references or pool errors.
3. **Check Pooling**: Confirm `GenericPickup` count in pool increases/decreases correctly.

## Tasks
- [ ] Task 1: Create `ILootFactory.cs` and `LootFactory.cs`.
- [ ] Task 2: Refactor `LootSpawner.cs` to use `LootFactory`.
- [ ] Task 3: Improve distribution logic in `LootSpawner`.
- [ ] Task 4: (Optional) Implement Stacking logic for high-quantity drops.
- [ ] Task 5: Verify with Ogre Boss.
