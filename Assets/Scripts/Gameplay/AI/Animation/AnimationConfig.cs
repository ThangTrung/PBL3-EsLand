using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.AI.Animation
{
    [System.Serializable]
    public struct AnimationSequence
    {
        public string stateName;
        public Sprite[] frames;
        public bool isLooping;
                public int[] multiTriggerFrames; // Thêm mảng cho phép nhiều frame nổ dame
public int triggerFrame;
    }

    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "Enemies/Animation Config")]
    public class AnimationConfig : ScriptableObject
    {
        [SerializeField] private List<AnimationSequence> animations = new List<AnimationSequence>();
        [SerializeField] private float frameRate = 10f;

        public float FrameRate => frameRate;

        public AnimationSequence? GetSequence(string stateName)
        {
            foreach (var seq in animations)
            {
                if (seq.stateName == stateName)
                    return seq;
            }
            return null;
        }

        // Backward compatibility for existing systems
        public Sprite[] IdleFrames => GetSequence(Gameplay.AI.Animation.AnimationStateNames.Idle)?.frames;
        public Sprite[] RunFrames => GetSequence(Gameplay.AI.Animation.AnimationStateNames.Run)?.frames;
        public Sprite[] AttackFrames => GetSequence(Gameplay.AI.Animation.AnimationStateNames.Attack)?.frames;
        public Sprite[] DeathFrames => GetSequence(Gameplay.AI.Animation.AnimationStateNames.Death)?.frames;
        public int AttackTriggerFrame => GetSequence(Gameplay.AI.Animation.AnimationStateNames.Attack)?.triggerFrame ?? -1;

        public void Initialize(List<AnimationSequence> sequences, float rate)
        {
            animations = sequences;
            frameRate = rate;
        }
    }
}
