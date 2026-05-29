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
        [SerializeField] private float flashDuration = 0.2f;
        
        private SpriteRenderer _spriteRenderer;
        private Coroutine _flashRoutine;

        public int Priority => 0; // Dodge runs first

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public float ModifyDamage(float incomingDamage, Character source)
        {
            if (Random.value <= dodgeChance)
            {
                TriggerDodgeFeedback();
                return 0f;
            }
            return incomingDamage;
        }

        private void TriggerDodgeFeedback()
        {
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(DodgeFlashRoutine());
            
            // Logic for a "Miss!" popup or ghost effect could go here
            // For now, a distinct semi-transparent flash is a professional starting point
        }

        private IEnumerator DodgeFlashRoutine()
        {
            if (_spriteRenderer == null) yield break;
            
            Color originalColor = _spriteRenderer.color;
            _spriteRenderer.color = dodgeFlashColor;
            
            // Shift position slightly for a "blur" or "ghost" feel
            Vector3 originalPos = _spriteRenderer.transform.localPosition;
            _spriteRenderer.transform.localPosition += new Vector3(0.2f, 0f, 0f);

            yield return new WaitForSeconds(flashDuration);

            _spriteRenderer.color = originalColor;
            _spriteRenderer.transform.localPosition = originalPos;
            _flashRoutine = null;
        }

        public void SetDodgeChance(float chance) => dodgeChance = chance;
    }
}