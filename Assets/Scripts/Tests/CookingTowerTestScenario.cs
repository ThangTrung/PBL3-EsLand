using Data.Items;
using Gameplay.Building;
using UnityEngine;

namespace Tests
{
    public class CookingTowerTestScenario : MonoBehaviour
    {
        [Header("References")]
        public CookingTower cookingTower;

        [Header("Test Data")]
        public MaterialItem inputMeat;
        public ItemData woodFuel;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("[Test] Thêm 5 Gỗ vào Lò.");
                cookingTower.SetTestItem(1, woodFuel, 5);
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                Debug.Log("[Test] Thêm 2 Thịt Sống vào Lò.");
                cookingTower.SetTestItem(0, inputMeat, 2);
            }
            
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Debug.Log($"[Test] Trạng thái lò: Fuel={cookingTower.CurrentFuelTime:F1}s, Cook={cookingTower.CookingProgress:F1}s");
                Debug.Log($"[Test] Slots: In={cookingTower.Slots[0].Amount}, Fuel={cookingTower.Slots[1].Amount}, Out={cookingTower.Slots[2].Amount}");
            }
        }
    }
}
