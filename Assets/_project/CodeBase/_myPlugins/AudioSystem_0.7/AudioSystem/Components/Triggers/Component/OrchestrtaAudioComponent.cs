using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Events;

using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    public class OrchestrtaAudioComponent : MonoBehaviour
    {
        [Header("Audio Assets")]
        [SerializeField] 
        private AudioEventAsset _eventAsset;
        [SerializeField] 
        private AudioParameterAsset _speedRtpcAsset;
        [SerializeField] 
        private float _fadeDuration = 0.5f;
        [SerializeField] 
        private bool _isLoop = true;

        [SerializeField]
        private Collider _collider;

        private IAudioMakerStateService _stateService;

        [Inject]
        private void Construct(IAudioMakerStateService stateService)
        {
            _stateService = stateService;

            if(_collider == null)
                _collider = GetComponent<Collider>();

            _collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var maker = other.GetComponentInParent<IAudioMaker>();
            if (maker != null)
                _stateService.EnterState(maker, _eventAsset, _speedRtpcAsset, _fadeDuration, _isLoop);
        }

        private void OnTriggerExit(Collider other)
        {
            var maker = other.GetComponentInParent<IAudioMaker>();
            if (maker != null)
                _stateService.ExitState(maker, _eventAsset, _fadeDuration, _isLoop);
        }
    }

}
