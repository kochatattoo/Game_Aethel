using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
namespace Code.Hero.Attributes
{
    public sealed class Attribute : IDisposable
    {
        private readonly List<AttributeModifier> _modifiers = new();
        private readonly IntReactiveProperty _finalValue = new();
        public IReadOnlyReactiveProperty<int> FinalValue => _finalValue;
        public int BaseValue { get; private set; }
        public int ModifierCount => _modifiers.Count;
        public Attribute(int baseValue) { SetBaseValue(baseValue); }
        public void SetBaseValue(int value) { if(value < 0) throw new ArgumentOutOfRangeException(nameof(value)); BaseValue=value; Recalculate(); }
        public void AddBaseValue(int amount) => SetBaseValue(BaseValue + amount);
        public void AddModifier(AttributeModifier modifier) { if(modifier == null) throw new ArgumentNullException(nameof(modifier)); if(!_modifiers.Contains(modifier)) { _modifiers.Add(modifier); Recalculate(); } }
        public void RemoveModifier(AttributeModifier modifier) { if(_modifiers.Remove(modifier)) Recalculate(); }
        public void RemoveSource(string source) { if(_modifiers.RemoveAll(m=>m.SourceKey==source)>0) Recalculate(); }
        public void ReplaceSource(string source, IEnumerable<AttributeModifier> modifiers)
        {
            if(string.IsNullOrWhiteSpace(source)) throw new ArgumentException(nameof(source));
            var replacement = new List<AttributeModifier>();
            if(modifiers != null) foreach(var m in modifiers) { if(m == null || m.SourceKey != source) throw new ArgumentException("Неверный источник модификатора"); replacement.Add(m); }
            _modifiers.RemoveAll(m=>m.SourceKey==source); _modifiers.AddRange(replacement); Recalculate();
        }
        public void ResetModifiers() { _modifiers.Clear(); Recalculate(); }
        private void Recalculate()
        {
            float flat=BaseValue, positive=0, negative=1;
            foreach(var m in _modifiers)
                if(m.Type==ModifierType.Flat) flat+=m.Amount;
                else if(m.Amount>=0) positive+=m.Amount/100f;
                else negative*=1+m.Amount/100f;
            _finalValue.Value=Mathf.Max(0,Mathf.RoundToInt(flat*(1+positive)*negative));
        }
        public void Dispose() => _finalValue.Dispose();
    }
}

