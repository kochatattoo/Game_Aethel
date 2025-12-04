using CodeBase.Enemies;
using CodeBase.Logic;
using System;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyAnimator))]
    public class EnemyHealth : MonoBehaviour, IHealth
    {
        public EnemyAnimator Animator;
        public Attack Attack;

        [SerializeField] private float _current;
        [SerializeField] private float _max;

        public float Current { get => _current; set => _current = value; }
        public float Max { get => _max; set => _max = value; }

        public event Action HealthChanged;

        public void TakeDamage(float damage)
        {
            Current -= damage;

            Animator.PlayHit();

            HealthChanged?.Invoke();
            Attack.ResetAttack();
        }
    }
}