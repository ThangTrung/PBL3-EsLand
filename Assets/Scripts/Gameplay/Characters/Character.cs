using Core.Contracts.Equipment;
using Core.Contracts.Inventory;
using UnityEngine;

namespace Gameplay.Characters
{
    [RequireComponent(typeof(CharacterHealth))]
    public class Character : MonoBehaviour, IInventoryHolder
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
            Inventory = GetComponent<IInventory>() ?? GetComponentInChildren<IInventory>();
            EquipmentManager = GetComponent<IEquipmentController>() ?? GetComponentInChildren<IEquipmentController>();
            Health = GetComponent<CharacterHealth>();
            Animator = GetComponentInChildren<Animator>();
            
            EquipmentManager?.Initialize(this);
            
            Debug.Log($"Character '{name}' initialized. Inv: {Inventory != null}, Equip: {EquipmentManager != null}, HP: {Health != null}");
        }
    }
}