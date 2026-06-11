using AudioSystem.Components.Interfaces.Components;
using AudioSystem.Room;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    [RequireComponent(typeof(Collider))]
    public class PortalOpeningComponent : MonoBehaviour, IPortalOpening
    {
        [SerializeField]
        private IndoorRoomComponent _parentRoom;
        [SerializeField]
        private bool _isOpen = true;
        [SerializeField]
        private Collider _collider;

        private IIndoorRoomService _service;

        public bool IsOpen => _isOpen;
        public Vector3 Position => transform.position;
        public IndoorRoomComponent Room => _parentRoom;

        private void Awake()
        {
            if(_collider == null)
                _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }

        [Inject]
        private void Construct(IIndoorRoomService indoorRoomService) =>
            _service = indoorRoomService;

        private void Start()
        {
            if (_parentRoom == null)
                _parentRoom = GetComponentInParent<IndoorRoomComponent>();

            _service.RegisterPortal(this);
        }

        private void OnDestroy() =>
            _service?.UnregisterPortal(this);

        public void SetOpen(bool open) => _isOpen = open;
    }
}
