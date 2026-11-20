using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Hero
{
    /// <summary>Адаптер характеристик героя к сохранению и HUD.</summary>
    public sealed class HeroCombatStats : MonoBehaviour, ISavedProgressReader
    {
        public float Damage { get; private set; }

        public void LoadProgress(PlayerProgress progress)
        {
            if (progress?.HeroStats != null) Damage = progress.HeroStats.Damage;
        }
    }
}
