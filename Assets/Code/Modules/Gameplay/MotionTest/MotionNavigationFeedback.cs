using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.MotionTest
{
    public sealed class MotionNavigationFeedback : MonoBehaviour
    {
        private readonly Vector3[] _corners = new Vector3[65];
        private MotionTestConfig _config;
        private GameObject _markerPrefab;
        private GameObject _root;
        private GameObject _marker;
        private LineRenderer _path;
        private bool _wasMoving;

        public bool PathVisible => _path != null && _path.positionCount > 1;
        public bool MarkerVisible => _marker != null && _marker.activeSelf;
        public float RemainingPathLength { get; private set; }

        public void Bind(MotionTestConfig config, GameObject markerPrefab)
        {
            _config = config;
            _markerPrefab = markerPrefab;
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
            _wasMoving = false;
            var count = path != null ? path.GetCornersNonAlloc(_corners) : 0;
            if (count < 2)
            {
                Clear();
                return;
            }
            DrawPath(null, count);
            ShowMarker(destination);
        }

        public void Tick(NavMeshAgent agent, bool moving)
        {
            if (moving)
            {
                _wasMoving = true;
                if (agent != null && agent.isOnNavMesh && !agent.pathPending && agent.path != null)
                {
                    var count = agent.path.GetCornersNonAlloc(_corners);
                    if (count > 1) DrawPath(agent.transform.position, count);
                }
                return;
            }
            if (_wasMoving) Clear();
        }

        public void Clear()
        {
            _wasMoving = false;
            RemainingPathLength = 0;
            if (_path != null) _path.positionCount = 0;
            if (_marker != null) Destroy(_marker);
            _marker = null;
        }

        private void DrawPath(Vector3? actorPosition, int cornerCount)
        {
            var first = 0;
            var closest = actorPosition ?? _corners[0];
            if (actorPosition.HasValue && cornerCount > 2)
            {
                var bestDistance = float.MaxValue;
                for (var i = 0; i < cornerCount - 1; ++i)
                {
                    var point = ClosestPoint(closest, _corners[i], _corners[i + 1]);
                    var distance = (point - closest).sqrMagnitude;
                    if (distance >= bestDistance) continue;
                    bestDistance = distance;
                    first = i;
                    _corners[cornerCount] = point;
                }
                closest = _corners[cornerCount];
            }

            var lineCount = cornerCount - first;
            _path.positionCount = lineCount;
            _path.SetPosition(0, closest + Vector3.up * .12f);
            RemainingPathLength = 0;
            for (var i = first + 1; i < cornerCount; ++i)
            {
                var target = _corners[i] + Vector3.up * .12f;
                _path.SetPosition(i - first, target);
                RemainingPathLength += Vector3.Distance(_path.GetPosition(i - first - 1), target);
            }
        }

        private static Vector3 ClosestPoint(Vector3 point, Vector3 start, Vector3 end)
        {
            var segment = end - start;
            var lengthSquared = segment.sqrMagnitude;
            if (lengthSquared < .0001f) return start;
            var t = Mathf.Clamp01(Vector3.Dot(point - start, segment) / lengthSquared);
            return start + segment * t;
        }

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
