// using System.Collections.Generic;
// using System.Linq;
// using Script.Inventory.Controller;
// using Script.Items;
// using UnityEngine;
//
// namespace Script.Entities
// {
//     public class Character : MonoBehaviour
//     {
//         [Header("Character Info")]
//         [SerializeField] protected string characterName;
//
//         [Header("Base Stats")]
//         [SerializeField] protected float maxHealth = 100f;
//         [SerializeField] protected float baseDamage = 10f;
//         [SerializeField] protected float baseDefense = 0f;
//         [SerializeField] protected float baseMoveSpeed = 5f;
//         [SerializeField] protected float baseRunSpeed = 8f;
//         [SerializeField] protected float baseAttackCooldown = 1f;
//
//         protected float CurrentHealth { get; set; }
//
//         private float _currentAttackCooldown;
//         private bool _isDead;
//
//         [Header("Inventory & Equipment")]
//         [SerializeField] protected InventoryController inventory;
//
//         private Tool _mainHandSlot;
//         //private readonly List<Armor> _equippedArmor = new List<Armor>();
//
//         protected virtual void Awake()
//         {
//             CurrentHealth = maxHealth;
//         }
//
//         protected virtual void Update()
//         {
//             if (_currentAttackCooldown > 0)
//             {
//                 _currentAttackCooldown -= Time.deltaTime;
//             }
//         }
//
//         public virtual void TakeDamage(float amount)
//         {
//             if (_isDead) return;
//
//             var finalDamage = Mathf.Max(0, amount - GetTotalDefense());
//             CurrentHealth -= finalDamage;
//
//             Debug.Log($"{gameObject.name} nhận {finalDamage} sát thương. Máu còn: {CurrentHealth}");
//
//             if (CurrentHealth <= 0)
//             {
//                 Die();
//             }
//         }
//
//         public virtual void Heal(float amount)
//         {
//             if (_isDead) return;
//             CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
//         }
//
//         protected virtual void Die()
//         {
//             _isDead = true;
//             Debug.Log($"{gameObject.name} đã chết.");
//         }
//
//         public float GetTotalDamage()
//         {
//             var total = baseDamage;
//             if (_mainHandSlot != null) total += _mainHandSlot.CombatDamage;
//             return total;
//         }
//
//         private float GetTotalDefense()
//         {
//             //return baseDefense + _equippedArmor.Sum(armor => armor.DefensePower);
//         }
//
//         public virtual bool Equip(Items.Equipment equipment)
//         {
//             switch (equipment)
//             {
//                 case Tool tool:
//                 {
//                     if (_mainHandSlot != null) _mainHandSlot.OnUnequip(this);
//                     _mainHandSlot = tool;
//                     _mainHandSlot.OnEquip(this);
//                     return true;
//                 }
//                 case Armor armor:
//                 {
//                     var existing = _equippedArmor.Find(a => a.EquipSlot == armor.EquipSlot);
//                     if (existing != null)
//                     {
//                         existing.OnUnequip(this);
//                         _equippedArmor.Remove(existing);
//                     }
//                 
//                     _equippedArmor.Add(armor);
//                     armor.OnEquip(this);
//                     return true;
//                 }
//                 default:
//                     return false;
//             }
//         }
//
//         public virtual bool Unequip(Items.Equipment equipment)
//         {
//             switch (equipment)
//             {
//                 case Tool tool when _mainHandSlot == tool:
//                     _mainHandSlot.OnUnequip(this);
//                     _mainHandSlot = null;
//                     return true;
//                 case Armor armor when _equippedArmor.Contains(armor):
//                     armor.OnUnequip(this);
//                     _equippedArmor.Remove(armor);
//                     return true;
//                 default:
//                     return false;
//             }
//         }
//     }
// }