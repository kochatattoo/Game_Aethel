using Infrastructure.AudioSystem.Utils;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components.Triggers
{
    /// <summary>
    /// Компонент-триггер, который принудительно устанавливает состояние переключателя (Wwise Switch) для входящего объекта.
    /// </summary>
    public class SurfaceAudioTrigger : MonoBehaviour
    {
        // TODO - НА данный момент не используется (подумать и доделать)
        // Может потребоваться в будущем для дизайнеров

        [SerializeField, AudioKeyAttribute(AudioKeyType.Switch)] 
        private string _switch;
        [SerializeField] 
        private string _targetTag = "Player";

        private IAudioFacade _audioFacade;

        [Inject]
        private void Construct(IAudioFacade audioFacade) => _audioFacade = audioFacade;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_targetTag))
            {
                _audioFacade.SetSwitch(_switch, other.gameObject);
            }
        }
    }
}
