using System;
using PetProject.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.MotionTest
{
    public sealed class MotionNavigationFeedback : MonoBehaviour
    {
        private readonly Vector3[] _corners = new Vector3[65];
        private readonly NavigationPoint[] _navigationCorners = new NavigationPoint[65];
        private MotionTestConfig _config;
        private GameObject _markerPrefab;
        private GameObject _root;
        private GameObject _marker;
        private LineRenderer _path;
        private INavigationLifecycle _lifecycle;

        public bool PathVisible => _path != null && _path.positionCount > 1;
        public bool MarkerVisible => _marker != null && _marker.activeSelf;
        public float RemainingPathLength { get; private set; }
        public NavigationLifecycleState LifecycleState => _lifecycle.State;

        public void Bind(MotionTestConfig config, GameObject markerPrefab, INavigationLifecycle lifecycle)
        {
            _config = config;
            _markerPrefab = markerPrefab;
            _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
            _root = new GameObject("Navigation feedback");
            _root.transform.SetParent(transform, false);
            _path = _root.AddComponent<LineRenderer>();
            _path.useWorldSpace = true;
            _path.sharedMaterial = config.GridMaterial;
            _path.startWidth = .09f;
            _path.endWidth = .055f;
            _path.numCapVertices = 3;
            _path.numCornerVertices = 4;
            _path.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _path.receiveShadows = false;
            _path.positionCount = 0;
        }

        public void ShowPath(NavMeshPath path, Vector3 destination)
        {
            _lifecycle.Begin();
            var count = path != null ? path.GetCornersNonAlloc(_corners) : 0;
            if (count < 2)
            {
                Clear();
                return;
            }
            CopyCorners(count);
            DrawPath(null, count);
            ShowMarker(destination);
        }

        public void Tick(NavMeshAgent agent, bool moving)
        {
            if (moving && agent != null && agent.isOnNavMesh && !agent.pathPending && agent.path != null)
            {
                var count = agent.path.GetCornersNonAlloc(_corners);
                if (count > 1)
                {
                    CopyCorners(count);
                    DrawPath(ToNavigation(agent.transform.position), count);
                }
            }
            if (_lifecycle.Update(moving)) Clear();
        }

        public void Clear()
        {
            _lifecycle.Reset();
            RemainingPathLength = 0;
            if (_path != null) _path.positionCount = 0;
            if (_marker != null) Destroy(_marker);
            _marker = null;
        }

        private void CopyCorners(int count)
        {
            for (var i = 0; i < count; ++i) _navigationCorners[i] = ToNavigation(_corners[i]);
        }

        private void DrawPath(NavigationPoint? actorPosition, int cornerCount)
        {
            var segment = new ArraySegment<NavigationPoint>(_navigationCorners, 0, cornerCount);
            var remaining = RemainingPathCalculator.Calculate(segment, actorPosition);
            var first = remaining.FirstCornerIndex;
            _path.positionCount = cornerCount - first;
            _path.SetPosition(0, ToUnity(remaining.Start) + Vector3.up * .12f);
            RemainingPathLength = remaining.Length;
            for (var i = first + 1; i < cornerCount; ++i)
                _path.SetPosition(i - first, _corners[i] + Vector3.up * .12f);
        }

        private static NavigationPoint ToNavigation(Vector3 point) => new NavigationPoint(point.x, point.y, point.z);
        private static Vector3 ToUnity(NavigationPoint point) => new Vector3(point.X, point.Y, point.Z);

        private void ShowMarker(Vector3 destination)
        {
            if (_marker != null) Destroy(_marker);
            _marker = _markerPrefab != null
                ? Instantiate(_markerPrefab, destination + Vector3.up * .08f, Quaternion.identity, _root.transform)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _marker.name = "Movement destination";
            if (_markerPrefab == null)
            {
                _marker.transform.SetParent(_root.transform, false);
                _marker.transform.position = destination + Vector3.up * .08f;
                _marker.transform.localScale = Vector3.one * .22f;
                var markerCollider = _marker.GetComponent<Collider>();
                if (markerCollider != null) Destroy(markerCollider);
                _marker.GetComponent<Renderer>().sharedMaterial = _config.GridMaterial;
            }
            else
            {
                _marker.transform.localScale *= .5f;
            }
        }

        private void OnDestroy()
        {
            if (_root != null) Destroy(_root);
        }
    }
}
