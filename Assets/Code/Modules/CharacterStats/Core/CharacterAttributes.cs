using System;

namespace PetProject.CharacterStats
{
    public enum CharacteristicType { Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma }

    public interface IReadOnlyCharacterAttributes
    {
        int Strength { get; }
        int Dexterity { get; }
        int Constitution { get; }
        int Intelligence { get; }
        int Wisdom { get; }
        int Charisma { get; }
        int Get(CharacteristicType type);
    }

    public readonly struct CharacterAttributes : IReadOnlyCharacterAttributes
    {
        public int Strength { get; }
        public int Dexterity { get; }
        public int Constitution { get; }
        public int Intelligence { get; }
        public int Wisdom { get; }
        public int Charisma { get; }

        public CharacterAttributes(int strength, int dexterity, int constitution, int intelligence,
            int wisdom, int charisma)
        {
            Strength = Validate(strength, nameof(strength));
            Dexterity = Validate(dexterity, nameof(dexterity));
            Constitution = Validate(constitution, nameof(constitution));
            Intelligence = Validate(intelligence, nameof(intelligence));
            Wisdom = Validate(wisdom, nameof(wisdom));
            Charisma = Validate(charisma, nameof(charisma));
        }

        public int Get(CharacteristicType type) => type switch
        {
            CharacteristicType.Strength => Strength,
            CharacteristicType.Dexterity => Dexterity,
            CharacteristicType.Constitution => Constitution,
            CharacteristicType.Intelligence => Intelligence,
            CharacteristicType.Wisdom => Wisdom,
            CharacteristicType.Charisma => Charisma,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        public static int Modifier(int value) => (int)Math.Floor((value - 10) * .5);

        private static int Validate(int value, string name)
        {
            if (value < 1 || value > 30) throw new ArgumentOutOfRangeException(name);
            return value;
        }
    }
}
