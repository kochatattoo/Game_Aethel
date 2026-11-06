using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Configs;
using UniRx;
using UnityEngine;
using Zenject;

namespace CodeBase.Components.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class EntityAudio : MonoBehaviour
    {
        private IAudioService _audioService;
        protected readonly CompositeDisposable Disposables = new CompositeDisposable();

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        /// <summary>
        /// Запрашивает источник из пула и привязывает его к текущему объекту
        /// </summary>
        protected void PlaySfx(AudioConfig config, bool follow = true)
        {
            if (config == null || config.Clip == null) return;

            _audioService.PlayAtTarget(config, transform, follow);
        }

        protected virtual void OnDestroy() => 
            Disposables.Dispose();
    }

}
