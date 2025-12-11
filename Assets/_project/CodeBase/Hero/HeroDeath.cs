using System;
using UnityEngine;

namespace CodeBase.Hero
{
    [RequireComponent(typeof(HeroHealth))]
    public class HeroDeath : MonoBehaviour
    {
        public HeroHealth Health;
        public HeroAttack Attack;

        public HeroMove Move;
        public HeroAnimator Animator;

        public GameObject DeathFx;
        private bool _isDead;

        public event Action PlayerDie;

        private void Start()
        {
            Health.HealthChanged += HealtChanged;
        }

        private void OnDestroy() =>
            Health.HealthChanged -= HealtChanged;

        private void HealtChanged()
        {
            if (_isDead == false && Health.Current <= 0)
                Die();
        }

        private void Die()
        {
            _isDead = true;
            Move.enabled = false;
            Attack.enabled = false;
            Animator.PlayDeath();
            Instantiate(DeathFx, transform.position, Quaternion.identity);

            PlayerDie?.Invoke();
        }
    }
}

