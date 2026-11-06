using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Zones;
using Shared.Utils;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    [RequireComponent(typeof(Collider))]
    public partial class IndoorRoomComponent : MonoBehaviour, IIndoorRoom
    {
        [SerializeField] 
        private LayerMask _emitterLayerMask = ~0;
        [SerializeField]
        private Collider _collider;

        private IAudioZoneRegistry _zoneRegistry;

        public Collider RoomCollider => _collider;
        public EventWrapper<IOcclusionEmitter> OnEmitterEntered { get; } = new();
        public EventWrapper<IOcclusionEmitter> OnEmitterExited { get; } = new();
        public EventWrapper<IndoorRoomListener> OnListenerEntered { get; } = new();
        public EventWrapper<IndoorRoomListener> OnListenerExited { get; } = new();


        private void Awake()
        {
            if(_collider == null)
               _collider = GetComponent<Collider>();

            _collider.isTrigger = true;
        }

        [Inject]
        private void Construct(IAudioZoneRegistry indoorRoomService) => _zoneRegistry= indoorRoomService;

        private void Start()
        {
            _zoneRegistry?.RegisterIndoorRoom(this);

            // Находим все IOcclusionEmitter, уже находящиеся в зоне действия коллайдера
            var hits = Physics.OverlapBox(_collider.bounds.center, _collider.bounds.extents, transform.rotation, _emitterLayerMask);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IOcclusionEmitter>(out var emitter))
                    OnEmitterEntered.Publish(emitter);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IOcclusionEmitter>(out var emitter)) 
                OnEmitterEntered.Publish(emitter);

            if (other.TryGetComponent<IndoorRoomListener>(out var listener))
                OnListenerEntered.Publish(listener);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IOcclusionEmitter>(out var emitter)) 
                OnEmitterExited.Publish(emitter);

            if (other.TryGetComponent<IndoorRoomListener>(out var listener))
                OnListenerExited.Publish(listener);
        }

        private void OnDestroy() => _zoneRegistry.UnregisterIndoorRoom(this);
    }
}
