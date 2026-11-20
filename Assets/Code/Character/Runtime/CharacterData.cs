using System;
using Code.Items;

namespace Code.Character.Data
{
    public sealed class CharacterData : IDisposable
    {
        public CharacterClass Character { get; }
        public EquipmentService Equipment { get; } = new();
        public BuffService Buffs { get; } = new();
        public CharacterDerivedStats Stats { get; }

        public CharacterData(CharacterClass character, CharacterBalance balance = null)
        {
            Character = character;
            Stats = new CharacterDerivedStats(character, balance ?? new CharacterBalance());
        }

        public void Dispose()
        {
            Buffs.Dispose();
            Equipment.Dispose();
            Stats.Dispose();
            Character.Dispose();
        }
    }
}
