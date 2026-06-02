# Plan: Survival Debuffs Implementation

## Phase 1: Core Logic in `PlayerSurvivalController`
1. Add debuff settings (threshold, multipliers, interval).
2. Implement health loss timer logic in `Update`.
3. Expose helper methods/properties for other controllers to check for debuffs.

## Phase 2: Applying Penalties
1. Modify `PlayerMovementController.GetMoveSpeed()` to multiply by survival speed multiplier.
2. Modify `PlayerInteractionController.GetTotalDamage()` to multiply by survival damage multiplier.

## Phase 3: Verification & Polish
1. Test and adjust drain rates for easier testing.
2. Ensure `OnDamageTaken` is correctly fired in `CharacterHealth` when called from `PlayerSurvivalController`.
3. Check if the "Hit" animation is triggered on the Player animator.

## Tasks
- [ ] Task 1: Update `PlayerSurvivalController.cs` with debuff and starvation logic.
- [ ] Task 2: Update `PlayerMovementController.cs` to apply speed penalty.
- [ ] Task 3: Update `PlayerInteractionController.cs` to apply damage penalty.
- [ ] Task 4: (Optional) Add a temporary debug command/key to drain stats for testing.
