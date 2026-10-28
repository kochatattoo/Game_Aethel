using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Code.Hero.Attributes
{
    public class HeroAttributes: IDisposable
    {
        private readonly Dictionary<string, Attribute> _attributes = new();

        public IReadOnlyDictionary<string, Attribute> AllAttributes => _attributes;
        public IReadOnlyReactiveProperty<int> this[string name]
        {
            get
            {
                if (_attributes.TryGetValue(name, out var attr))
                    return attr.FinalValue;

                Debug.LogError($"Attribute '{name}' not found");
                return null;
            }
        }

        public HeroAttributes(Dictionary<string, int> baseValues) 
        {
            foreach (KeyValuePair<string, int> kvp in baseValues)
            {
                _attributes.Add(kvp.Key, new Attribute(kvp.Value));
            }
        }

        public void Dispose()
        {
            foreach (var attr in _attributes.Values)
                attr.Dispose();

            _attributes.Clear();
        }

        /// <summary>
        /// Измененение базового значения
        /// </summary>
        public void AddBaseValue(string name, int amount)
        {
            if (_attributes.TryGetValue(name, out var attr))
                attr.AddBaseValue(amount);
        }

        /// <summary>
        /// Работа с модификаторами, добавление
        /// </summary>
        public void AddModifier(string attributeName, AttributeModifier modifier)
        {
            if (_attributes.TryGetValue(attributeName, out var attr))
                attr.AddModifier(modifier); 
        }

        /// <summary>
        /// Работа с модификаторами, удаление
        /// </summary>
        public void RemoveModifier(string attributeName, AttributeModifier modifier)
        {
            if (_attributes.TryGetValue(attributeName, out var attr))
                attr.RemoveModifier(modifier);
        }

        public int GetValue(string name) => _attributes.TryGetValue(name, out var attr) ? attr.FinalValue.Value : 0;
    }
}
