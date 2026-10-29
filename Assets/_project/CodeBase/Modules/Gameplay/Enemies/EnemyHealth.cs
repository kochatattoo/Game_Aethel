using CodeBase.Logic;
using System;
using UniRx;
using UnityEngine;

namespace CodeBase.Enemies
{
    [RequireComponent(typeof(EnemyAnimator))]
    public class EnemyHealth : MonoBehaviour, IHealth
    {
        public EnemyAnimator Animator;
        public Attack Attack;

        [Header("Settings")]
        [SerializeField] private float _invulnerabilityDuration = 0.5f; // Длительность заморозки
        private float _nextAllowedDamageTime;

        private readonly FloatReactiveProperty _current = new();
        private readonly FloatReactiveProperty _max = new();

        public IReadOnlyReactiveProperty<float> Current => _current;
        public IReadOnlyReactiveProperty<float> Max => _max;

        float IHealth.Current { get => _current.Value; set => _current.Value = value; }
        float IHealth.Max { get => _max.Value; set => _max.Value = value; }

        // Оставляем для старых систем, если нужно
        public event Action HealthChanged;

        public void TakeDamage(float damage)
        {
            // 1. Если HP уже 0 или мы в режиме "заморозки" — выходим
            if (_current.Value <= 0 || Time.time < _nextAllowedDamageTime)
                return;

            // 2. Устанавливаем время следующего возможного получения урона
            _nextAllowedDamageTime = Time.time + _invulnerabilityDuration;

            _current.Value = Mathf.Max(0, _current.Value - damage);

            Animator.PlayHit();
            Attack.ResetAttack();

            HealthChanged?.Invoke();
        }
    }
}