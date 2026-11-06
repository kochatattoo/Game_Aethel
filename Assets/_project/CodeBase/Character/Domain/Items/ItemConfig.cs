using Code.Hero.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Hero.Items
{
    public abstract class ItemConfig : ScriptableObject
    {
        public string ItemName;
        public string Description;
        public Sprite Icon;
        public List<StatModifierData> Modifiers;

        [System.Serializable]
        public struct StatModifierData
        {
            public AttributeName Attribute; // Например, AttributeConstants.STR (возможно стот от string уйти к enum)
            public int Amount;
            public ModifierType Type; // Flat или Percentage

            public string GetKey() => Attribute.ToStringValue();
        }
    }
}
