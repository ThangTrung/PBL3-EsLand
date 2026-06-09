# Spec: Fix Missing Boss Loot & Refactor Loot System

## Objective
Fix the issue where items dropped by Bosses (and potentially other enemies) are not visible or fail to appear. Refactor the loot system to comply with project standards (Object Pooling, SOLID) and handle high-volume drops efficiently.

## Analysis of Root Causes
1. **Instantiate Violation:** Current `LootSpawner` uses `GameObject.Instantiate`, which causes performance spikes and violates `GEMINI.md`.
2. **Physics Explosion:** Dropping 100+ items at the same position causes extreme physics collisions, shooting items out of bounds.
3. **Layer/Sorting Issues:** Items might be spawning behind the background or with incorrect sorting orders.
4. **Lifecycle Mismatch:** Enemies are returned to the pool immediately after triggering loot, which might interrupt spawning or parenting logic.

## Tech Stack
- Unity 2D
- C# (SOLID Principles)
- ObjectPoolManager (Existing Infrastructure)

## Commands
- **Test Logic:** Run in-editor play mode and kill the Ogre Boss.
- **Verification:** Check `ObjectPoolManager` stats to ensure `GenericPickup` is being pooled.

## Project Structure
- `Assets/Scripts/Core/Contracts/Gameplay/ILootStrategy.cs` (New)
- `Assets/Scripts/Gameplay/World/LootSpawner.cs` (Refactor)
- `Assets/Scripts/Gameplay/World/LootFactory.cs` (New)

## Code Style
- Use `Strategy Pattern` for different drop behaviors.
- Adhere to `ObjectPoolManager.Instance.Get` and `Return`.
- XML Documentation for all public methods.

## Testing Strategy
1. **Unit Test:** Verify `LootFactory` returns correct amount of items from pool.
2. **Integration Test:** Verify Ogre Boss triggers `LootSpawner` and items appear in scene.
3. **Stress Test:** Verify 150+ items drop without crashing or "exploding" away.

## Boundaries
- **Always do:** Use Object Pooling for all spawned pickups.
- **Ask first:** If we need to change the `ItemData` structure or `IEnemyConfig`.
- **Never do:** Use `Instantiate` or `Destroy` in the gameplay loop.

## Success Criteria
- Ogre Boss drops visible loot upon death.
- Items are distributed naturally around the death point without exploding.
- No `Instantiate` calls in `LootSpawner`.
- Performance remains stable during high-volume drops.

## Open Questions
- Should we aggregate small items (like Gold) into "Stacks" if the count is too high?
- Should the loot drop be triggered via Animation Event instead of `OnAnimationFinished`?
