using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    [RequireComponent(typeof(AkGameObj))]
    public class AmbientEnvironmentBinder : MonoBehaviour
    {
        [SerializeField] 
        private EnvironmentAudioComponent _environmentZone;

        private IAudioFacade _audioFacade;

        [Inject]
        private void Construct(IAudioFacade audioFacade)
        {
            _audioFacade = audioFacade;
        }

        private void Start()
        {
            ApplyEnvironment();
        }

        private void ApplyEnvironment()
        {
            if (_environmentZone == null || _environmentZone.AuxBus == null)
                return;

            _audioFacade?.SetGameObjectAuxSend(gameObject, _environmentZone.AuxBus);
        }

        // Опционально: если объект может менять зону (например, движущаяся платформа с амбиенсом)
        public void SetEnvironmentZone(EnvironmentAudioComponent newZone)
        {
            _environmentZone = newZone;
            ApplyEnvironment();
        }

        private void OnDestroy()
        {
            _audioFacade?.ResetGameObjectAuxSend(gameObject);
        }
    }

}
