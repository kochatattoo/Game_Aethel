using System;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroDeath : IDisposable
    {
        private readonly Transform _transform;
        private readonly HeroHealth _health;
        private readonly HeroAttack _attack;

        private readonly Move _move;
        private readonly HeroAnimator _animator;

        private readonly GameObject DeathFx;
        private bool _isDead;

        public event Action PlayerDie;

        public HeroDeath(Transform transform, HeroHealth health, HeroAttack attack, Move move, HeroAnimator animator, GameObject deathFx)
        {
            _transform = transform;
            _health = health;
            _attack = attack;
            _move = move;
            _animator = animator;
            DeathFx = deathFx;
        }

        public void Initialize()
        {
            _health.HealthChanged += HealtChanged;
        }

        public void Dispose() =>
            _health.HealthChanged -= HealtChanged;

        private void HealtChanged()
        {
            if (_isDead == false && _health.Current <= 0)
                Die();
        }

        private void Die()
        {
            _isDead = true;
            _move.enabled = false;
            _attack.enabled = false;
            _animator.PlayDeath();
            GameObject.Instantiate(DeathFx, _transform.position, Quaternion.identity);

            PlayerDie?.Invoke();
        }
    }
}

