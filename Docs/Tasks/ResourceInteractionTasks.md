# Tasks: Resource Interaction Implementation

## Phase 1: Resource System Refinement
- [x] Task: Refactor `ResourceNode.cs` to trigger "Hit" animator parameter and invoke events consistently.
  - Acceptance: `TakeDamage` calls `OnDamaged` and sets "Hit" trigger if animator exists.
  - Verify: Inspection of `ResourceNode.cs`.
  - Files: `Assets/Scripts/Gameplay/World/ResourceNode.cs`
- [x] Task: Ensure `ResourceVisualEffects.cs` correctly hooks into `OnDamaged`.
  - Acceptance: `ResourceVisualEffects` starts shake coroutine on damage.
  - Verify: Inspection of `ResourceVisualEffects.cs`.
  - Files: `Assets/Scripts/Gameplay/Environment/ResourceVisualEffects.cs`

## Phase 2: Player Controller Updates
- [x] Task: Enhance `PlayerMovementController.cs` for robust target arrival.
  - Acceptance: `FixedUpdate` correctly detects collider touching to trigger callback.
  - Verify: Inspection of `PlayerMovementController.cs`.
  - Files: `Assets/Scripts/Gameplay/Characters/PlayerMovementController.cs`
- [x] Task: Update `PlayerInteractionController.cs` sequence.
  - Acceptance: `InteractWithTarget` correctly coordinates movement, facing, and interaction.
  - Verify: Inspection of `PlayerInteractionController.cs`.
  - Files: `Assets/Scripts/Gameplay/Characters/PlayerInteractionController.cs`
- [x] Task: Validate `PlayerInputController.cs` click priority.
  - Acceptance: Raycast finds `IInteractable` and initiates targeted interaction.
  - Verify: Inspection of `PlayerInputController.cs`.
  - Files: `Assets/Scripts/Gameplay/Characters/PlayerInputController.cs`

## Phase 3: Final Verification
- [x] Task: Final code review and compilation check.
  - Acceptance: Zero compilation errors and logic follows SOLID.
  - Verify: `read_console` for errors.
  - Files: All touched scripts.
