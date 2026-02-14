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
            _current.Value = Mathf.Max(0, _current.Value - damage);

            Animator.PlayHit();
            Attack.ResetAttack();

            HealthChanged?.Invoke();
        }
    }
}