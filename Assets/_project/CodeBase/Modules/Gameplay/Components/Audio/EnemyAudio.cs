using CodeBase.Configs;
using CodeBase.Enemies;
using System.Linq;
using UniRx;
using UnityEngine;
using ZLinq;

namespace CodeBase.Components.Audio
{
    public class EnemyAudio : EntityAudio
    {
        [Header("Enemy SFX")]
        [SerializeField] private AudioConfig _hitSfx;
        [SerializeField] private AudioConfig _deathSfx;
        [SerializeField] private AudioConfig _attackSfx;

        public void Construct(EnemyHealth health, EnemyDeath death, Attack attack)
        {
            Disposables.Clear();

            // Звук урона
            health.Current
                .Pairwise()
                .Where(pair => pair.Current < pair.Previous && pair.Current > 0)
                .Subscribe(_ => PlaySfx(_hitSfx))
                .AddTo(Disposables);

            // Звук смерти
            death.OnDeath
                .Subscribe(_ => PlaySfx(_deathSfx))
                .AddTo(Disposables);

            // Звук атаки
            //attack.Attacked
            //    .Subscribe(_ => PlaySfx(_attackSfx))
            //    .AddTo(Disposables);
        }
      
    }
}
