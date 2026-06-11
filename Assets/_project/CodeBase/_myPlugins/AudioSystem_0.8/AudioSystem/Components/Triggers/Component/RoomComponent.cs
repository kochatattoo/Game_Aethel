using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Zones;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    [RequireComponent(typeof(Collider))]
    public class RoomComponent : MonoBehaviour, IEnvironmentComponent, IRoomComponent
    {
        [SerializeField]
        private AudioAuxBusAsset _roomAuxBus;
        [SerializeField]
        private int _priority;
        [SerializeField]
        private Collider _collider;

        private ulong _roomId;
        private IAudioRegistrator _audioRegistrator;
        private IAudioZoneRegistry _zoneRegistry;

        public ulong RoomID => _roomId;
        public AK.Wwise.AuxBus AuxBus => _roomAuxBus.WwiseAuxBus;
        public int Priority => _priority;

        [Inject]
        private void Construct(IAudioRegistrator audioRegistrator, IAudioZoneRegistry zoneRegistry)
        {
            _audioRegistrator = audioRegistrator;
            _zoneRegistry = zoneRegistry;
        }

        private void Awake()
        {
            if (_collider == null)
                _collider = GetComponent<Collider>();

            _collider.isTrigger = true;
        }

        private void Start()
        {
            _zoneRegistry?.RegisterRoom(this);
            RegisterRoom();
        }

        public void RegisterRoom()
        {
            if (_audioRegistrator == null)
                return;

            _audioRegistrator.RegisterRoom(this);
        }

        public void UnregisterRoom()
        {
            if (_audioRegistrator == null)
                return;

            _audioRegistrator.UnregisterRoom(this);
        }

        public void SetRoomId(ulong id) => _roomId = id;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IAudioEnviromentMaker>(out var maker))
            {
                maker.EnviromentResolver.EnterRoom(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IAudioEnviromentMaker>(out var maker))
            {
                maker.EnviromentResolver.ExitRoom(this);
            }
        }

        private void OnDestroy()
        {
            _zoneRegistry?.UnregisterRoom(this);
            UnregisterRoom();
        }
    }
}
