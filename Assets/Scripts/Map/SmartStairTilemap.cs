using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class SmartStairTilemap : MonoBehaviour
    {
        public string lowerTierLayer;
        public int lowerOrder;
        public string upperTierLayer;
        public int upperOrder;

        private Dictionary<Collider2D, float> entryYPositions = new Dictionary<Collider2D, float>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                entryYPositions[other] = other.transform.position.y;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && entryYPositions.ContainsKey(other))
            {
                float exitY = other.transform.position.y;
                float entryY = entryYPositions[other];

                SpriteRenderer spriteRenderer = other.GetComponentInChildren<SpriteRenderer>();

                if (exitY > entryY) // Đi lên
                {
                    other.gameObject.layer = LayerMask.NameToLayer(upperTierLayer);
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = upperOrder;
                    }
                }
                else // Đi xuống
                {
                    other.gameObject.layer = LayerMask.NameToLayer(lowerTierLayer);
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = lowerOrder;
                    }
                }

                entryYPositions.Remove(other);
            }
        }
    }
}