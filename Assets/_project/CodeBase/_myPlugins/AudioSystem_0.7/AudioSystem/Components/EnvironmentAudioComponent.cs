using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Events;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components
{
    [RequireComponent(typeof(Collider))]
    public class EnvironmentAudioComponent : MonoBehaviour
    {
        [Header("Wwise Environment")]
        [SerializeField]
        private AudioAuxBusAsset _auxBusAsset; // Шина реверберации в Wwise

        [SerializeField]
        private Collider _collider;

        [SerializeField]
        private int _priority = 0; // Для выбора между накладывающимися зонами

        public AK.Wwise.AuxBus AuxBus => _auxBusAsset.WwiseAuxBus;
        public int Priority => _priority;

        private void Awake()
        {
            if(_collider == null) 
              _collider = GetComponent<Collider>();

            _collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Возможно стоит перевести на
            // var maker = other.GetComponentInParent<IAudioEnviromentMaker>();
            // maker?.EnviromentResolver.EnterEnvironment(this);
            // Но это пока не точно

            if (other.TryGetComponent<IAudioEnviromentMaker>(out var maker))
            {
                maker.EnviromentResolver.EnterEnvironment(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IAudioEnviromentMaker>(out var maker))
            {
                maker.EnviromentResolver.ExitEnvironment(this);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            AudioEditorGizmos.DrawZoneBounds(this, new Color(0, 1, 0, 0.3f));
        }

        private void OnDrawGizmosSelected()
        {
            AudioEditorGizmos.DrawZoneBounds(this, Color.green);
        }
#endif
    }
}
