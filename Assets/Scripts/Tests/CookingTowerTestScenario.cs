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
                cookingTower.SetTestItem(1, woodFuel, 5);
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                cookingTower.SetTestItem(0, inputMeat, 2);
            }
            
            if (Input.GetKeyDown(KeyCode.F3))
            {
            }
        }
    }
}
