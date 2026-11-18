using System;

namespace PetProject.Tactics
{
    public sealed class ActorDefinition
    {
        public string Id { get; }
        public string FactionId { get; }
        public GridPosition Position { get; }
        public int MaxHealth { get; }
        public int InitiativeBonus { get; }
        public int AttackBonus { get; }
        public int Defense { get; }
        public int Damage { get; }
        public int? InitialActionPoints { get; }
        public int AttackDiceSides { get; }
        public int DamageDiceCount { get; }
        public int DamageDiceSides { get; }
        public int DamageBonus { get; }
        public bool UsesDamageDice => DamageDiceCount > 1 || DamageDiceSides > 1;

        public ActorDefinition(string id, string factionId, GridPosition position, int maxHealth = 12,
            int initiativeBonus = 0, int attackBonus = 3, int defense = 12, int damage = 4,
            int? initialActionPoints = null, int attackDiceSides = 20, int damageDiceCount = 1,
            int damageDiceSides = 1, int damageBonus = 0)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Actor ID is required", nameof(id));
            if (string.IsNullOrWhiteSpace(factionId)) throw new ArgumentException("Faction ID is required", nameof(factionId));
            if (maxHealth < 1) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (damage < 1) throw new ArgumentOutOfRangeException(nameof(damage));
            if (initialActionPoints < 0) throw new ArgumentOutOfRangeException(nameof(initialActionPoints));
            if (attackDiceSides < 2) throw new ArgumentOutOfRangeException(nameof(attackDiceSides));
            if (damageDiceCount < 1) throw new ArgumentOutOfRangeException(nameof(damageDiceCount));
            if (damageDiceSides < 1) throw new ArgumentOutOfRangeException(nameof(damageDiceSides));
            Id = id;
            FactionId = factionId;
            Position = position;
            MaxHealth = maxHealth;
            InitiativeBonus = initiativeBonus;
            AttackBonus = attackBonus;
            Defense = defense;
            Damage = damage;
            InitialActionPoints = initialActionPoints;
            AttackDiceSides = attackDiceSides;
            DamageDiceCount = damageDiceCount;
            DamageDiceSides = damageDiceSides;
            DamageBonus = damageBonus;
        }
    }
}
