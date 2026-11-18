using System;

namespace PetProject.CharacterStats
{
    public readonly struct DiceDefinition
    {
        public int Count { get; }
        public int Sides { get; }
        public int Bonus { get; }
        public bool UsesDice => Count > 1 || Sides > 1;

        public DiceDefinition(int count, int sides, int bonus = 0)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
            if (sides < 1) throw new ArgumentOutOfRangeException(nameof(sides));
            Count = count;
            Sides = sides;
            Bonus = bonus;
        }
    }
}
