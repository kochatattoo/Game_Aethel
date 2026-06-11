using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Components.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components
{
    [RequireComponent(typeof(Collider))]
    public class RoomPortalComponent : MonoBehaviour, IPortalComponent
    {
        [SerializeField]
        private LayerMask _roomLayerMask = ~0;
        [SerializeField]
        private Vector3 _axis = Vector3.right;
        [SerializeField]
        private Collider _collider;

        private IEnvironmentComponent _roomA;
        private IEnvironmentComponent _roomB;

        private readonly List<IAudioEnviromentMaker> _trackedMakers = new();

        private void Awake()
        {
            if(_collider  == null)
               _collider = GetComponent<Collider>();

            _collider.isTrigger = true;
        }

        private void Start()
        {
            var bounds = _collider.bounds;
            var hits = Physics.OverlapBox(bounds.center, bounds.extents, transform.rotation, _roomLayerMask);
            var rooms = new List<IEnvironmentComponent>();
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];

                if (hit.TryGetComponent<IEnvironmentComponent>(out var room) && !rooms.Contains(room))
                    rooms.Add(room);

                if (rooms.Count >= 2)
                    break;
            }

            if (rooms.Count == 2)
            {
                _roomA = rooms[0];
                _roomB = rooms[1];
            }

            else
            {
                Debug.LogWarning($"[RoomPortal] Expected 2 rooms, found {rooms.Count}. Disabling.");
                enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            other.TryGetComponent<IAudioEnviromentMaker>(out var maker);

            if (maker == null || _trackedMakers.Contains(maker))
                return;

            _trackedMakers.Add(maker);

            maker.PortalResolver.EnterPortal(this, _roomA, _roomB);   // PortalResolver обрабатывает IEnvironmentComponent
            maker.EnviromentResolver.IsInPortal = true;
        }

        private void OnTriggerExit(Collider other)
        {
            var maker = other.GetComponentInParent<IAudioEnviromentMaker>();
            if (maker == null || !_trackedMakers.Remove(maker))
                return;

            maker.PortalResolver.ExitPortal(this);
            maker.EnviromentResolver.IsInPortal = false;
            maker.EnviromentResolver.UpdateAuxSend();
        }

        public float CalculateWeight(Vector3 position)
        {
            // Аналогично EnvironmentPortalComponent
            Vector3 localPos = transform.InverseTransformPoint(position);
            float proj = Vector3.Dot(localPos, _axis.normalized);
            Vector3 localCenter = transform.InverseTransformPoint(_collider.bounds.center);
            float centerProj = Vector3.Dot(localCenter, _axis.normalized);
            float half = Vector3.Dot(_collider.bounds.extents, _axis.normalized);
            float min = centerProj - half;
            float max = centerProj + half;
            return Mathf.InverseLerp(min, max, proj);
        }

        public float CalculateDistance(Vector3 position) =>
             Vector3.Distance(_collider.ClosestPoint(position), position);
    }
}
