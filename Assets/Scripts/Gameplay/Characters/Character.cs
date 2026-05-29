using Core.Contracts.Equipment;
using Core.Contracts.Inventory;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Base class for all characters (Player, Enemies, NPCs).
    /// Provides common accessors for shared components like Health, Inventory, and Animator.
    /// </summary>
    [RequireComponent(typeof(CharacterHealth))]
    public abstract class Character : MonoBehaviour, IInventoryHolder
    {
        [Header("Character Information")]
        [SerializeField] protected string characterName = "New Character";

        public string CharacterName => characterName;
        
        public virtual IInventory Inventory { get; protected set; }
        public virtual IEquipmentController EquipmentManager { get; protected set; }
        public CharacterHealth Health { get; private set; }
        public Animator Animator { get; private set; }
        
        protected virtual void Awake()
        {
            // Robust component finding: Check root then children
            Inventory = GetComponent<IInventory>() ?? GetComponentInChildren<IInventory>();
            EquipmentManager = GetComponent<IEquipmentController>() ?? GetComponentInChildren<IEquipmentController>();
            
            Health = GetComponent<CharacterHealth>();
            Animator = GetComponentInChildren<Animator>();
            
            EquipmentManager?.Initialize(this);
            
        }
    }
}
