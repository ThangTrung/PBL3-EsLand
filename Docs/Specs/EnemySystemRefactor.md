# Spec: Enemy System Improvements (Targeting, Looting, Spawning)

## Objective
To finalize the refactoring of the Enemy System by addressing three critical areas:
1.  **Target Detection Performance:** Eliminate expensive `GameObject.FindGameObjectWithTag("Player")` calls across all enemies.
2.  **Loot Dropping:** Ensure enemies drop physical items upon death (similar to trees dropping wood) that the player can pick up by walking over them, utilizing the existing `LootSpawner` and item pickup logic.
3.  **Spawner & Factory Refactoring:** Remove unsafe, string-based `Resources.Load` fallbacks from `EnemyBase` and strictly enforce the Factory pattern for dependency injection.

## Tech Stack
- Unity (C#)
- Object Pooling (Infrastructure.Pooling)
- `LootSpawner` and `GameEvents` (Core.Events)

## Project Structure
- `Assets/Scripts/Gameplay/AI/EnemyBase.cs`
- `Assets/Scripts/Gameplay/World/LootSpawner.cs`
- `Assets/Scripts/Gameplay/Spawning/EnemySpawnDirector.cs`
- `Assets/Scripts/Core/Events/GameEvents.cs`
- `Assets/Scripts/Gameplay/Characters/TargetTracker.cs` (New)

## Code Style
Strict adherence to SOLID principles. Dependency Injection via Interfaces. No hardcoded string comparisons for logic. 

```csharp
// Example: Event-driven target tracking
public class TargetTracker : MonoBehaviour 
{
    public static Transform PlayerTarget { get; private set; }
    
    private void OnEnable() => GameEvents.OnPlayerReady += SetTarget;
    private void OnDisable() => GameEvents.OnPlayerReady -= SetTarget;
    
    private void SetTarget(IInventoryHolder player) => PlayerTarget = (player as Component)?.transform;
}
```

## Testing Strategy
- **Manual Verification:** Play mode testing to confirm enemies smoothly chase the player without spamming `FindObjectWithTag`.
- **Loot Drop:** Kill an enemy, ensure it drops a physical `pickupPrefab` with the correct `ItemData` based on the Enemy's config, and that the player can pick it up.
- **Spawn Consistency:** Verify that `EnemySpawnDirector` correctly spawns enemies through `EnemyFactory` and no `MissingReferenceException` occurs.

## Boundaries
- **Always:** Use `ObjectPoolManager` for any instantiated item/enemy.
- **Always:** Rely on `GameEvents` or a global Tracker for referencing the Player.
- **Never:** Use `Resources.Load` with string concatenation in `Awake/Start` of a frequently spawned object like an Enemy.
- **Never:** Send Loot directly to the inventory if physical drops are expected (let the existing `ItemPickup` trigger handle the inventory logic).

## Implementation Plan (Tasks)

1.  **Task 1: Centralized Target Tracking**
    - Create a simple `PlayerTargetTracker` (or utilize an existing manager) that listens to `GameEvents.OnPlayerReady` and stores the Player's `Transform`.
    - Refactor `EnemyBase.FindTarget()` and `EnemySpawnDirector` to query this tracker instead of calling `GameObject.FindGameObjectWithTag("Player")`.

2.  **Task 2: Dynamic Loot Spawning Integration**
    - Modify `EnemyBase.TriggerLootDrop()`.
    - If a `LootSpawner` exists on the enemy, dynamically clear and populate its `lootTable` using `ConfigInternal.LootItemId` and `ConfigInternal.LootQuantity`.
    - To map `LootItemId` (string) to `ItemData` (ScriptableObject), we may need to load the `ItemData` from a central database or Resources, but ideally, this mapping should be efficient. We will implement a safe lookup.
    - Call `spawner.SpawnLoot()`.
    - Remove the redundant `GameEvents.InvokeEnemyDroppedLoot(lootData)` if the actual physical `ItemPickup` script already handles adding to the inventory upon collision.

3.  **Task 3: Enforce Factory Pattern (Remove TryAutoInitialize)**
    - Remove `TryAutoInitialize` from `EnemyBase` completely to prevent string-based `Resources.Load` overhead.
    - Ensure `EnemyFactory` is the sole authority for initializing an Enemy.
    - Ensure `EnemySpawnDirector` properly utilizes `EnemyFactory.Instance.CreateEnemy()`.

## Success Criteria
- [ ] No `FindGameObjectWithTag` calls exist in `EnemyBase` or `EnemySpawnDirector`.
- [ ] Dying enemies drop physical objects (like trees drop wood) corresponding to their Config's Loot Data.
- [ ] Player can pick up the dropped loot via the existing proximity pickup system.
- [ ] No `Resources.Load` calls exist inside `EnemyBase.Awake()`.
- [ ] The game runs without null reference exceptions when enemies spawn and die.