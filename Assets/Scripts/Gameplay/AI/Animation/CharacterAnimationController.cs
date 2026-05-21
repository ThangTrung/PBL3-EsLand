using System;
using UnityEngine;

namespace Gameplay.AI.Animation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterAnimationController : MonoBehaviour
    {
        public enum AnimState
        {
            Idle,
            Run,
            Attack,
            Death
        }

        public event Action<int, AnimState> OnFrameChanged;

        [SerializeField] private AnimationConfig config;

        private SpriteRenderer _spriteRenderer;
        private AnimState _currentState = AnimState.Idle;
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

        public void PlayIdle()
        {
            if (_currentState == AnimState.Idle) return;
            SetAnimation(AnimState.Idle, config != null ? config.IdleFrames : null, true);
        }

        public void PlayRun()
        {
            if (_currentState == AnimState.Run) return;
            SetAnimation(AnimState.Run, config != null ? config.RunFrames : null, true);
        }

public float PlayAttack()
        {
            SetAnimation(AnimState.Attack, config != null ? config.AttackFrames : null, false);
            if (config != null && config.AttackFrames != null && config.FrameRate > 0f)
            {
                return (float)config.AttackFrames.Length / config.FrameRate;
            }
            return 0.5f; // Fallback
        }

        public void PlayDeath()
        {
            SetAnimation(AnimState.Death, config != null ? config.DeathFrames : null, false);
        }

        public void SetFacingByMove(Vector3 direction)
        {
            if (direction.x == 0) return;
            var scale = transform.localScale;
            scale.x = Mathf.Sign(direction.x);
            transform.localScale = scale;
        }

        public AnimState GetCurrentState() => _currentState;

        public int GetCurrentFrameIndex() => _currentFrameIndex;

        public bool IsCurrentAnimationFinished() => _isFinished;

        public void SetConfig(AnimationConfig animationConfig)
        {
            config = animationConfig;
            if (_currentState == AnimState.Idle)
            {
                SetAnimation(AnimState.Idle, config != null ? config.IdleFrames : null, true);
            }
        }

        private void SetAnimation(AnimState state, Sprite[] frames, bool loop)
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
