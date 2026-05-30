using Core.Contracts.Combat;
using Gameplay.Characters;
using UnityEngine;
using System.Collections;

namespace Gameplay.AI.Strategies.Modifiers
{
    public class DodgeModifier : MonoBehaviour, IDamageModifier
    {
        [SerializeField, Range(0, 1)] private float dodgeChance = 0.2f;
        [Header("Feedback")]
        [SerializeField] private Color dodgeFlashColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float flashDuration = 0.15f;
        
        private SpriteRenderer _spriteRenderer;
        private Coroutine _flashRoutine;

        public int Priority => 0; // Dodge runs first

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public float ModifyDamage(float incomingDamage, Character source)
        {
            if (incomingDamage > 0 && Random.value <= dodgeChance)
            {
                TriggerDodgeFeedback();
                return 0f;
            }
            return incomingDamage;
        }

        private void TriggerDodgeFeedback()
        {
            if (!gameObject.activeInHierarchy) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(DodgeFlashRoutine());
        }

        private IEnumerator DodgeFlashRoutine()
        {
            if (_spriteRenderer == null) yield break;
            
            Color originalColor = _spriteRenderer.color;
            _spriteRenderer.color = dodgeFlashColor;
            
            // PROFESSIONAL FEEDBACK: Use a slight scale pulse instead of position offset
            // Position offset can cause physics glitches if applied to wrong transform
            Vector3 originalScale = _spriteRenderer.transform.localScale;
            _spriteRenderer.transform.localScale = originalScale * 1.1f;

            yield return new WaitForSeconds(flashDuration);

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = originalColor;
                _spriteRenderer.transform.localScale = originalScale;
            }
            _flashRoutine = null;
        }

        public void SetDodgeChance(float chance) => dodgeChance = chance;
    }
}