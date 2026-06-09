# Spec: Survival Debuffs and Starvation

## Objective
Implement survival penalties for the player when hunger levels are critical. This adds realism and challenge to the survival gameplay.

### User Stories
- As a player, when my hunger is below 10%, I want to feel sluggish (slower movement) and weaker (less damage) so that I'm motivated to find food.
- As a player, when my hunger reaches 0%, I want my health to slowly drain and see visual feedback (hit effects) so that I know I'm dying.

## Tech Stack
- Unity 2022.3+ (Existing)
- C# / .NET (Existing)

## Commands
- Test: Use Unity Test Runner or manual playtesting.

## Project Structure
- `Assets/Scripts/Gameplay/Characters/PlayerSurvivalController.cs`: Core logic for managing debuffs and starvation.
- `Assets/Scripts/Gameplay/Characters/PlayerMovementController.cs`: Consume speed multiplier from survival state.
- `Assets/Scripts/Gameplay/Characters/PlayerInteractionController.cs`: Consume damage multiplier from survival state.

## Code Style
- Follow existing project conventions (PascalCase for public, camelCase for private with underscore prefix).
- SOLID principles: Separation of concerns. `PlayerSurvivalController` manages the state, others consume it.

## Testing Strategy
- Manual testing:
    1. Drain hunger using a debug tool or high drain rates.
    2. Verify movement speed reduction at < 10%.
    3. Verify damage reduction at < 10% (hitting resources/enemies).
    4. Verify health loss at 0% (1 HP every 5 seconds).
    5. Verify hit flash effect triggers during health loss.

## Boundaries
- Always: Check for null references.
- Ask first: Changing base stats or global constants.
- Never: Hardcode values that should be in Inspector.

## Success Criteria
- [ ] Hunger < 10% triggers 50% slow (configurable).
- [ ] Hunger < 10% triggers 50% damage reduction (configurable).
- [ ] Hunger == 0 triggers 1 HP damage every 5 seconds.
- [ ] Starvation damage triggers `OnDamageTaken` event, causing `CombatFeedbackController` to flash the character.

## Assumptions & Open Questions
1. **Cumulative or Boolean?**: If hunger is < 10%, penalties are active.
2. **Hit Direction**: Starvation damage has no source position. `CombatFeedbackController` handles this by choosing a random direction if `source == null`.
3. **Animations**: Does the player have a specific "Starving" animation? -> *Assumption: No, we just use the standard Hit animation/flash as requested.*
