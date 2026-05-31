namespace Gameplay.AI.Animation
{
    /// <summary>
    /// Constants for Animation States. 
    /// Prevents typos and makes refactoring easier across the AI system.
    /// </summary>
    public static class AnimationStateNames
    {
        public const string Idle = "Idle";
        public const string Run = "Run";
        public const string Attack = "Attack";
        public const string Death = "Death";
        public const string Defense = "Defense";
        
        // Turtle Specific
        public const string Defense_1 = "Defense_1";
        public const string Defense_2 = "Defense_2";
        
        // Boss Specific
        public const string Windup = "Windup";
        public const string Recovery = "Recovery";
    }
}