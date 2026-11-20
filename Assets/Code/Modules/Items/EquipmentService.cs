using System;
using System.Collections.Generic;
using Code.Character.Attributes;
using Code.Character.Data;
using UniRx;
namespace Code.Items
{
    public sealed class EquipmentService : IDisposable
    {
        private readonly ReactiveDictionary<EquipmentSlot,EquipmentItem> _slots=new();
        public IReadOnlyReactiveDictionary<EquipmentSlot,EquipmentItem> Slots=>_slots;
        public void Equip(CharacterClass hero,EquipmentItem item)
        {
            if(item==null)throw new ArgumentNullException(nameof(item));
            var source="equipment:"+item.Slot;
            foreach(var p in hero.Attributes.AllAttributes)
            {
                var mods=new List<AttributeModifier>();
                foreach(var data in item.Modifiers)if(data.GetKey()==p.Key)mods.Add(new AttributeModifier(data.Type,data.Amount,source));
                p.Value.ReplaceSource(source,mods);
            }
            _slots[item.Slot]=item;
        }
        public void Unequip(CharacterClass hero,EquipmentItem item)
        {
            if(!IsEquipped(item))return;
            foreach(var p in hero.Attributes.AllAttributes.Values)p.RemoveSource("equipment:"+item.Slot);
            _slots.Remove(item.Slot);
        }
        public bool IsEquipped(EquipmentItem item)=>item!=null && _slots.TryGetValue(item.Slot,out var value)&&value==item;
        public void Dispose()=>_slots.Dispose();
    }
}

