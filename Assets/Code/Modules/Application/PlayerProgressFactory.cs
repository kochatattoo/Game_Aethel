using Code.Character.Data;
using CodeBase.Data;

namespace CodeBase.CharacterIntegration
{
    internal static class PlayerProgressFactory
    {
        public static PlayerProgress Create(CharacterData hero, string initialLevel)
        {
            var progress = new PlayerProgress(initialLevel)
            {
                CharacterSchemaVersion = 1,
                Character = CharacterProfileData.Capture(hero.Character)
            };
            progress.HeroState.MaxHP = hero.Stats.MaxHealth.Value;
            progress.HeroState.ResetHP();
            progress.HeroStats.Damage = hero.Stats.Damage.Value;
            progress.HeroStats.DamageRadius = 1;
            return progress;
        }
    }
}
