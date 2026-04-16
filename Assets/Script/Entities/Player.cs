// using Script.Inventory.Controller;
// using UnityEngine;
//
// namespace Script.Entities
// {
//     public class Player : Character
//     {
//         [Header("Survival Stats")]
//         [SerializeField] private float maxHunger = 100f;
//         [SerializeField] private float maxThirst = 100f;
//         [SerializeField] private float maxStamina = 100f;
//
//         [Header("Components")]
//         [SerializeField] private InventoryController myInventory;
//
//         private float _hunger;
//         private float _thirst;
//         private float _stamina;
//
//         public float Hunger { get => _hunger; private set => _hunger = value; }
//         public float Thirst { get => _thirst; private set => _thirst = value; }
//         public float Stamina { get => _stamina; private set => _stamina = value; }
//
//         protected override void Awake()
//         {
//             base.Awake();
//             Hunger = maxHunger;
//             Thirst = maxThirst;
//             Stamina = maxStamina;
//             
//             if (myInventory == null)
//                 myInventory = GetComponentInChildren<InventoryController>(true);
//         }
//
//         protected override void Update()
//         {
//             base.Update();
//             HandleInventoryInput();
//         }
//
//         private void HandleInventoryInput()
//         {
//             if (Input.GetKeyDown(KeyCode.E) && myInventory != null)
//                 myInventory.ToggleDisplay();
//         }
//
//         public void Consume(float hungerAmount, float thirstAmount, float healthAmount)
//         {
//             Hunger = Mathf.Min(Hunger + hungerAmount, maxHunger);
//             Thirst = Mathf.Min(Thirst + thirstAmount, maxThirst);
//             Heal(healthAmount);
//             Debug.Log($"[Player] Consume -> Hunger:{Hunger:F0} Thirst:{Thirst:F0} HP:{CurrentHealth:F0}");
//         }
//
//         public void InteractWithObject(GameObject obj)
//         {
//             Debug.Log($"[Player] Tuong tac voi {obj.name}");
//         }
//     }
// }