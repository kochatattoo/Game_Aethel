using Infrastructure.AudioSystem.Events;
using UnityEngine;
using Zenject;


namespace Infrastructure.AudioSystem.Components.Triggers
{
    public abstract class BaseSwitchTriggerAudioComponent: MonoBehaviour
    {
        [SerializeField]
        protected AudioSwitchAsset _defaultSwitchAsset;
        [SerializeField]
        protected AudioSwitchAsset _audioSwitchAsset;
        [SerializeField]
        protected bool _isTriggerEnter = true;
        [SerializeField]
        protected bool _isTriggerExit = true;

        protected IAudioFacade _audioFacade;

        [Inject]
        private void Construct(IAudioFacade audioFacade) => _audioFacade = audioFacade;

        private void OnTriggerEnter(Collider other)
        {
            if (_isTriggerEnter)
            {
                OnEnter(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_isTriggerExit)
            {
                OnExit(other);
            }
        }

        protected abstract void OnEnter(Collider other);
        protected abstract void OnExit(Collider other);
    }
}
