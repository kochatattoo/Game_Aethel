using UnityEngine;

namespace Code.Hero.Items
{
    [CreateAssetMenu(fileName = "NewEquipment", menuName = "Items/Equipment")]
    public class EquipmentItem: ItemConfig
    {
        public EquipmentSlot Slot;
    }
}
