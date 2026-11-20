using UnityEngine;

namespace Code.Items
{
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "Items/Consumable")]
    public class ConsumableItem : ItemConfig
    {
        public float Duration; // Длительность эффекта в секундах
    }
}
