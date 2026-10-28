using UnityEngine;

namespace Code.Hero.Items
{
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "Items/Consumable")]
    public class ConsumableItem : ItemConfig
    {
        public float Duration; // Длительность эффекта в секундах
    }
}
