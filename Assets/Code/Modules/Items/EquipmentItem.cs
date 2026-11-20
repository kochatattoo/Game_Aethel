using UnityEngine;

namespace Code.Items
{
    [CreateAssetMenu(fileName = "NewEquipment", menuName = "Items/Equipment")]
    public class EquipmentItem: ItemConfig
    {
        public EquipmentSlot Slot;
    }
}
