# Spec: Wood Drop on Tree Chop Feature

## Objective
Implement a feature where chopping down a tree drops a wood item that flies out and can be picked up by the player. This provides visual feedback for gathering resources and integrates with the existing inventory system.

## Assumptions I'm Making
1. The game uses a 2D top-down perspective (using Rigidbody2D, Collider2D).
2. The logic for tree destruction (`TreeResource.cs`), loot spawning (`LootSpawner.cs`), flying items (`ItemPickup.cs`), and player picking (`PlayerPickup.cs`) already exists and is fully functional. We do not need to write new code, just configure Unity Assets and Components.
3. The Wood Sprite is located at `Assets/Prefabs/Item/Wood/Wood Resource.png`.

## Tech Stack
Unity 2022/2023 2D, C#

## Commands
No new commands. Configuration will be done manually in the Unity Editor Inspector.

## Project Structure
- `Assets/Data/Items/MaterialItem/Wood.asset`: The data defining the wood item.
- `Assets/Prefabs/Item/Wood/WoodPickup.prefab`: The item spawned in the world.
- `Assets/Prefabs/Resource/Tree/Tree/*.prefab`: The trees that drop the item.
- `Assets/Prefabs/Character/Player/*.prefab`: The player that can pick up the item.

## Code Style
No new code required. Reusing existing architecture (`LootSpawner`, `ItemPickup`, `PlayerPickup`, `MaterialItem`).

## Testing Strategy
1. Open a test scene in Unity Editor.
2. Ensure player has an inventory.
3. Chop down a tree.
4. Verify the Wood item pops out with physics.
5. Move the player near the wood.
6. Verify the Wood item flies towards the player and gets added to the inventory.

## Boundaries
- **Always:** Use existing `ItemPickup` and `LootSpawner` components instead of writing custom logic.
- **Never:** Hardcode item drops inside the `TreeResource.cs`.

## Success Criteria
- [ ] Tree dies -> spawns 1-3 Wood items.
- [ ] Wood item has a 2D physical bounce/flight upon spawning.
- [ ] Player coming close -> item flies to player and enters inventory.
- [ ] Item is destroyed after pickup.

## Tasks
- [ ] Task 1: Create `Wood` Item ScriptableObject in `Assets/Data/Items/MaterialItem/`.
- [ ] Task 2: Create `WoodPickup` prefab in `Assets/Prefabs/Item/Wood/`.
- [ ] Task 3: Configure `PlayerPickup` on the Player prefab.
- [ ] Task 4: Configure `LootSpawner` on the Tree prefabs.
