using UnityEngine;

namespace Gameplay.AI.Animation
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "Enemies/Animation Config")]
    public class AnimationConfig : ScriptableObject
    {
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] runFrames;
        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private Sprite[] deathFrames;
        [SerializeField] private float frameRate = 10f;
        [SerializeField] private int attackTriggerFrame = 2;

        public Sprite[] IdleFrames => idleFrames;
        public Sprite[] RunFrames => runFrames;
        public Sprite[] AttackFrames => attackFrames;
        public Sprite[] DeathFrames => deathFrames;
        public float FrameRate => frameRate;
        public int AttackTriggerFrame => attackTriggerFrame;

        public void Initialize(Sprite[] idle, Sprite[] run, Sprite[] attack, Sprite[] death, float rate, int triggerFrame)
        {
            idleFrames = idle;
            runFrames = run;
            attackFrames = attack;
            deathFrames = death;
            frameRate = rate;
            attackTriggerFrame = triggerFrame;
        }
    }
}
