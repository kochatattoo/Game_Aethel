using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using System;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroHealth : MonoBehaviour, ISavedProgress, IHealth
    {
        private HeroAnimator _animator;
        private State _state;

        [Header("Settings")]
        [SerializeField] 
        private float _invulnerabilityDuration = 0.5f; // Длительность заморозки
        private float _nextAllowedDamageTime; // Время, когда можно снова нанести урон

        public event Action HealthChanged;

        public float Current
        {
            get => _state.CurrentHP;
            set
            {
                if (_state.CurrentHP != value)
                {
                    _state.CurrentHP = value;
                    HealthChanged?.Invoke();
                }
            }
        }

        public float Max
        {
            get => _state.MaxHP;
            set => _state.MaxHP = value;
        }

        public void Construct(HeroAnimator animator) => 
            _animator = animator;

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

        public void TakeDamage(float damage)
        {
            if (Current <= 0 || Time.time < _nextAllowedDamageTime)
                return;

            _nextAllowedDamageTime = Time.time + _invulnerabilityDuration;

            Current -= damage;
            _animator.PlayHit();
        }
    }
}

