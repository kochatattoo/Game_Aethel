using System;
using PetProject.CharacterStats;

namespace PetProject.EnemyArchetypes
{
    public enum EnemyArchetypeKind { Skeleton, SkeletonArcher, SkeletonMage, SkeletonCaptain, SkeletonGeneral }
    public enum EnemyRank { Normal, Uncommon, Boss }

    public interface IEnemyArchetype
    {
        EnemyArchetypeKind Kind { get; }
        EnemyRank Rank { get; }
        string DisplayName { get; }
        IReadOnlyCombatStats Stats { get; }
        DiceDefinition AttackDice { get; }
        DiceDefinition DamageDice { get; }
    }

    public sealed class EnemyArchetypeDefinition : IEnemyArchetype
    {
        public EnemyArchetypeKind Kind { get; }
        public EnemyRank Rank { get; }
        public string DisplayName { get; }
        public IReadOnlyCharacterAttributes Attributes { get; }
        public IReadOnlyCombatStats Stats { get; }
        public DiceDefinition AttackDice { get; }
        public DiceDefinition DamageDice { get; }

        public EnemyArchetypeDefinition(EnemyArchetypeKind kind, EnemyRank rank, string displayName,
            IReadOnlyCharacterAttributes attributes, CombatStatsSettings settings,
            DiceDefinition attackDice, DiceDefinition damageDice)
        {
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required", nameof(displayName));
            Kind = kind;
            Rank = rank;
            DisplayName = displayName;
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            Stats = CombatStatsCalculator.Calculate(attributes, settings);
            AttackDice = attackDice;
            DamageDice = damageDice;
        }
    }
}
