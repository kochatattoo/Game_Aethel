using CodeBase.Configs;
using UnityEngine;

namespace CodeBase.Components.Audio
{
    public class PlayerAudio : EntityAudio
    {
        [Header("Player SFX")]
        [SerializeField] private AudioConfig _jumpSfx;
        [SerializeField] private AudioConfig _healSfx;

        // Предположим, у игрока есть свой провайдер событий
        //public void Construct(PlayerMovement move, PlayerHealth health)
        //{
        //    Disposables.Clear();

        //    move.Jumped
        //        .Subscribe(_ => PlaySfx(_jumpSfx))
        //        .AddTo(Disposables);

        //    health.Healed
        //        .Subscribe(_ => PlaySfx(_healSfx))
        //        .AddTo(Disposables);
        //}
    }
}
