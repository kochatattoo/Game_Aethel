using Infrastructure.AudioSystem.Events;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    /// <summary>
    /// Базовый класс для работы с компонентами в альтернативу классам от SDK Wwise
    /// </summary>
    [RequireComponent(typeof(AkGameObj))]
    public abstract class BaseAudioEventComponent: MonoBehaviour
    {
        [Header("Wwise настройки")]
        [SerializeField]
        private AudioEventAsset _wwiseEvent;

        [SerializeField] 
        protected bool _playOnStart = false;
        
        protected IAudioFacade _audioFacade;

        [Inject]
        private void Construct(IAudioFacade audioFacade) => _audioFacade = audioFacade;

        protected virtual void Start()
        {
            if (_playOnStart) 
                Play();
        }

        public virtual void Play()
        {
            _audioFacade.PostEvent(_wwiseEvent.WwiseEvent, gameObject);
        }

        public virtual void Stop()
        {
            _audioFacade.StopEvent(_wwiseEvent.WwiseEvent, gameObject);
        }

        protected virtual void OnDisable()
        {
            Stop();
        }
    }
}
