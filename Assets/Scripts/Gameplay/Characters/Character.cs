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
        protected virtual void Awake()
        {
            Inventory = GetComponentInChildren<IInventory>();
            EquipmentManager = GetComponent<IEquipmentController>();
        }
    }
}