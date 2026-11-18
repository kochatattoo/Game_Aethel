using System;

namespace PetProject.CharacterStats
{
    public interface IReadOnlyCombatStats
    {
        int MaxHealth { get; }
        int InitiativeBonus { get; }
        int AttackBonus { get; }
        int Defense { get; }
    }

    public readonly struct CombatStatsSettings
    {
        public int BaseMaxHealth { get; }
        public int HealthPerConstitutionModifier { get; }
        public int ProficiencyBonus { get; }
        public int BaseDefense { get; }
        public int ArmorBonus { get; }
        public CharacteristicType AttackCharacteristic { get; }

        public CombatStatsSettings(int baseMaxHealth, int healthPerConstitutionModifier, int proficiencyBonus,
            int baseDefense, int armorBonus, CharacteristicType attackCharacteristic)
        {
            if (baseMaxHealth < 1) throw new ArgumentOutOfRangeException(nameof(baseMaxHealth));
            if (healthPerConstitutionModifier < 0) throw new ArgumentOutOfRangeException(nameof(healthPerConstitutionModifier));
            if (proficiencyBonus < 0) throw new ArgumentOutOfRangeException(nameof(proficiencyBonus));
            if (baseDefense < 0) throw new ArgumentOutOfRangeException(nameof(baseDefense));
            if (armorBonus < 0) throw new ArgumentOutOfRangeException(nameof(armorBonus));
            BaseMaxHealth = baseMaxHealth;
            HealthPerConstitutionModifier = healthPerConstitutionModifier;
            ProficiencyBonus = proficiencyBonus;
            BaseDefense = baseDefense;
            ArmorBonus = armorBonus;
            AttackCharacteristic = attackCharacteristic;
        }
    }

    public readonly struct CombatStats : IReadOnlyCombatStats
    {
        public int MaxHealth { get; }
        public int InitiativeBonus { get; }
        public int AttackBonus { get; }
        public int Defense { get; }

        public CombatStats(int maxHealth, int initiativeBonus, int attackBonus, int defense)
        {
            MaxHealth = maxHealth;
            InitiativeBonus = initiativeBonus;
            AttackBonus = attackBonus;
            Defense = defense;
        }
    }

    public static class CombatStatsCalculator
    {
        public static CombatStats Calculate(IReadOnlyCharacterAttributes attributes, CombatStatsSettings settings)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));
            var constitutionModifier = CharacterAttributes.Modifier(attributes.Constitution);
            var dexterityModifier = CharacterAttributes.Modifier(attributes.Dexterity);
            var attackModifier = CharacterAttributes.Modifier(attributes.Get(settings.AttackCharacteristic));
            return new CombatStats(
                Math.Max(1, settings.BaseMaxHealth + constitutionModifier * settings.HealthPerConstitutionModifier),
                dexterityModifier,
                attackModifier + settings.ProficiencyBonus,
                settings.BaseDefense + settings.ArmorBonus + dexterityModifier);
        }
    }
}
