using System;
using CodeBase.CharacterIntegration;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Hero
{
    /// <summary>Состояние здоровья героя для HUD и сохранения.</summary>
    public sealed class HeroHealth : MonoBehaviour, ISavedProgress, IHealth
    {
        private State _state;
        private ICharacterStatsSource _character;

        public event Action HealthChanged;

        public float Current
        {
            get => _state?.CurrentHP ?? 0;
            set
            {
                if (_state == null || Mathf.Approximately(_state.CurrentHP, value)) return;
                _state.CurrentHP = value;
                HealthChanged?.Invoke();
            }
        }

        public float Max
        {
            get => _state?.MaxHP ?? 0;
            set
            {
                if (_state == null) return;
                _state.MaxHP = value;
                HealthChanged?.Invoke();
            }
        }

        public void BindStats(ICharacterStatsSource character)
        {
            if (_character != null) _character.StatsChanged -= NotifyStats;
            _character = character;
            _character.StatsChanged += NotifyStats;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _state = progress.HeroState;
            HealthChanged?.Invoke();
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.HeroState.CurrentHP = Current;
            progress.HeroState.MaxHP = Max;
        }

        private void NotifyStats() => HealthChanged?.Invoke();

        private void OnDestroy()
        {
            if (_character != null) _character.StatsChanged -= NotifyStats;
        }
    }
}
