using System;
namespace Code.Hero.Attributes
{
    public sealed class AttributeModifier
    {
        public ModifierType Type { get; }
        public int Amount { get; }
        public string SourceKey { get; }
        public AttributeModifier(ModifierType type, int amount, string sourceKey = null)
        {
            if (type == ModifierType.Percantage && amount < -100) throw new ArgumentOutOfRangeException(nameof(amount));
            Type = type; Amount = amount; SourceKey = sourceKey ?? Guid.NewGuid().ToString("N");
        }
    }
}

