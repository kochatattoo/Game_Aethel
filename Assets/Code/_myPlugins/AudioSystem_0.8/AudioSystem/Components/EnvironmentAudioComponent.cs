using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Zones;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    [RequireComponent(typeof(Collider))]
    public class EnvironmentAudioComponent : MonoBehaviour, IEnvironmentComponent
    {
        [Header("Wwise Environment")]
        [SerializeField]
        private AudioAuxBusAsset _auxBusAsset; // Шина реверберации в Wwise
        [SerializeField]
        private Collider _collider;
        [SerializeField]
        private int _priority = 0; // Для выбора между накладывающимися зонами

        private IAudioZoneRegistry _zoneRegistry;

        public AK.Wwise.AuxBus AuxBus => _auxBusAsset.WwiseAuxBus;
        public int Priority => _priority;

        [Inject]
        private void Construct(IAudioZoneRegistry zoneRegistry) =>
            _zoneRegistry = zoneRegistry;

        private void Awake()
        {
            if(_collider == null)
              _collider = GetComponent<Collider>();

            _collider.isTrigger = true;
        }

        private void Start()
        {
            _zoneRegistry?.RegisterEnvironment(this);
        }

        private void OnTriggerEnter(Collider other)
        {
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

        private void OnDestroy()
        {
            _zoneRegistry?.UnregisterEnvironment(this);
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
