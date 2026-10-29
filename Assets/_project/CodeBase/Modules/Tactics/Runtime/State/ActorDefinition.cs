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

        public ActorDefinition(string id, string factionId, GridPosition position, int maxHealth = 12,
            int initiativeBonus = 0, int attackBonus = 3, int defense = 12, int damage = 4)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Actor ID is required", nameof(id));
            if (string.IsNullOrWhiteSpace(factionId)) throw new ArgumentException("Faction ID is required", nameof(factionId));
            if (maxHealth < 1) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (damage < 1) throw new ArgumentOutOfRangeException(nameof(damage));
            Id = id;
            FactionId = factionId;
            Position = position;
            MaxHealth = maxHealth;
            InitiativeBonus = initiativeBonus;
            AttackBonus = attackBonus;
            Defense = defense;
            Damage = damage;
        }
    }
}
