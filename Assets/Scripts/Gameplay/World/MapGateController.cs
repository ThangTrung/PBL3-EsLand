using UnityEngine;
using Gameplay.Characters;
using Infrastructure.SaveSystem.Core;
using Infrastructure.SaveSystem.Data;

namespace Gameplay.World
{
    /// <summary>
    /// Controls access to map areas by blocking them until a specific condition (e.g., boss death) is met.
    /// Integrates with the Save System to persist the unlocked state.
    /// </summary>
    public class MapGateController : MonoBehaviour, ISaveable
    {
        [Header("Gate Configuration")]
        [Tooltip("Unique ID for this gate to persist its state in the save file.")]
        [SerializeField] private string gateID = "Gate_Area1_To_Area2";
        
        [Tooltip("The Health component of the boss that needs to be defeated to unlock this gate.")]
        [SerializeField] private CharacterHealth targetBossHealth;
        
        [Header("Visuals & Feedback")]
        [Tooltip("The visual and collision objects of the gate that will be deactivated when unlocked.")]
        [SerializeField] private GameObject gateVisuals;
        
        [Tooltip("Optional particle effect to play when the gate is unlocked during gameplay.")]
        [SerializeField] private ParticleSystem unlockVfx;

        private bool _isUnlocked = false;

        private void Start()
        {
            // Register for the boss death event if not already unlocked
            if (!_isUnlocked && targetBossHealth != null)
            {
                targetBossHealth.OnDie += HandleBossDied;
            }
            
            // Set initial state
            UpdateGateState();
        }

        private void OnDestroy()
        {
            if (targetBossHealth != null)
                targetBossHealth.OnDie -= HandleBossDied;
        }

        private void HandleBossDied()
        {
            UnlockGate(true);
        }

        private void UnlockGate(bool withEffect)
        {
            if (_isUnlocked) return;
            
            _isUnlocked = true;

            if (withEffect && unlockVfx != null)
            {
                unlockVfx.Play();
            }

            UpdateGateState();
            
            Debug.Log($"<color=green>[MapGate]</color> Gate <b>{gateID}</b> has been unlocked!");
        }

        private void UpdateGateState()
        {
            if (gateVisuals != null)
            {
                gateVisuals.SetActive(!_isUnlocked);
            }
        }

        #region ISaveable Implementation
        public void LoadData(GameData data)
        {
            if (data.openedGates.Contains(gateID))
            {
                _isUnlocked = true;
                UpdateGateState();
            }
        }

        public void SaveData(GameData data)
        {
            if (_isUnlocked && !data.openedGates.Contains(gateID))
            {
                data.openedGates.Add(gateID);
            }
        }
        #endregion
    }
}
