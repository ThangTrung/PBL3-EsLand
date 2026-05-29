using System;
using UnityEngine;

namespace Gameplay.AI.Animation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterAnimationController : MonoBehaviour
    {
        public event Action<int, string> OnFrameChanged;

        [SerializeField] private AnimationConfig config;

        private SpriteRenderer _spriteRenderer;
        private string _currentState = Gameplay.AI.Animation.AnimationStateNames.Idle;
        private Sprite[] _currentFrames;
        private int _currentFrameIndex;
        private float _frameTimer;
        private bool _isLooping = true;
        private bool _isFinished;

        public AnimationConfig Config => config;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_currentFrames == null || _currentFrames.Length == 0) return;
            if (_isFinished) return;

            var frameDuration = config != null && config.FrameRate > 0f ? 1f / config.FrameRate : 0.1f;
            _frameTimer += Time.deltaTime;

            while (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;
                AdvanceFrame();
                if (_isFinished) break;
            }
        }

        /// <summary>
        /// Plays an animation sequence defined in AnimationConfig by name.
        /// </summary>
        /// <param name="stateName">The name of the animation state (e.g. "Roar")</param>
        /// <returns>The duration of the animation sequence, or 0.5f fallback.</returns>
        public float PlayAnimation(string stateName)
        {
            if (_currentState == stateName) return GetCurrentAnimationDuration();

            var seq = config != null ? config.GetSequence(stateName) : null;
            if (seq.HasValue)
            {
                SetAnimation(stateName, seq.Value.frames, seq.Value.isLooping);
                return GetCurrentAnimationDuration();
            }

            return 0.5f; // Fallback
        }

        private float GetCurrentAnimationDuration()
        {
            if (config != null && _currentFrames != null && config.FrameRate > 0f)
            {
                return (float)_currentFrames.Length / config.FrameRate;
            }
            return 0.5f; // Fallback
        }

        // --- Backward Compatibility Methods ---
        public void PlayIdle() => PlayAnimation(Gameplay.AI.Animation.AnimationStateNames.Idle);
        public void PlayRun() => PlayAnimation(Gameplay.AI.Animation.AnimationStateNames.Run);
        public float PlayAttack() => PlayAnimation(Gameplay.AI.Animation.AnimationStateNames.Attack);
        public void PlayDeath() => PlayAnimation(Gameplay.AI.Animation.AnimationStateNames.Death);
        // ---------------------------------------

        public void SetFacingByMove(Vector3 direction)
        {
            if (Mathf.Abs(direction.x) < 0.1f) return;
            var scale = transform.localScale;
            scale.x = Mathf.Sign(direction.x);
            transform.localScale = scale;
        }

        public string GetCurrentState() => _currentState;

        public int GetCurrentFrameIndex() => _currentFrameIndex;

        public bool IsCurrentAnimationFinished() => _isFinished;

        public void SetConfig(AnimationConfig animationConfig)
        {
            config = animationConfig;
            if (_currentState == Gameplay.AI.Animation.AnimationStateNames.Idle)
            {
                PlayIdle();
            }
        }

        private void SetAnimation(string state, Sprite[] frames, bool loop)
        {
            _currentState = state;
            _currentFrames = frames;
            _isLooping = loop;
            _isFinished = _currentFrames == null || _currentFrames.Length == 0;
            _currentFrameIndex = 0;
            _frameTimer = 0f;

            UpdateSprite();
            OnFrameChanged?.Invoke(_currentFrameIndex, _currentState);
        }

        private void AdvanceFrame()
        {
            if (_currentFrames == null || _currentFrames.Length == 0) return;

            _currentFrameIndex++;
            if (_currentFrameIndex >= _currentFrames.Length)
            {
                if (_isLooping)
                {
                    _currentFrameIndex = 0;
                }
                else
                {
                    _currentFrameIndex = _currentFrames.Length - 1;
                    _isFinished = true;
                }
            }

            UpdateSprite();
            OnFrameChanged?.Invoke(_currentFrameIndex, _currentState);
        }

        private void UpdateSprite()
        {
            if (_spriteRenderer == null || _currentFrames == null || _currentFrames.Length == 0) return;        
            _spriteRenderer.sprite = _currentFrames[_currentFrameIndex];
        }
    }
}
