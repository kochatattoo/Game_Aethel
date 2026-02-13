using CodeBase.Hero;
using System.Collections;
using UnityEngine;

namespace CodeBase.Enemies
{
    public class EnemyVision : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private Transform _headTransform;
        [SerializeField]
        private Follow _follow;

        [Header("Vision Settings")]
        [SerializeField]
        private float _viewDistance = 10f;
        [SerializeField, Range(0, 180), Tooltip("Горизонтальный угол обзора (ширина)")]
        private float _viewAngle = 150f;
        [SerializeField, Range(0, 180), Tooltip("Вертикальный угол обзора (высота)")] 
        private float _viewHeightAngle = 45f;

        [SerializeField]
        private LayerMask _playerMask;
        [SerializeField]
        private LayerMask _obstacleMask;
        [SerializeField]
        private float _cooldown = 2f;

        private readonly Collider[] _overlapBuffer = new Collider[1];
        private Coroutine _aggroCoroutine;
        private bool _hasTarget;

        private void Start()
        {
            if (_follow != null) _follow.enabled = false;

            if (_headTransform == null)
            {
                Debug.LogError($"[EnemyVision] на {gameObject.name} не назначен HeadTransform (пустышка глаз)!");
            }
        }

        private void Update()
        {
            if (_headTransform == null) return;

            int count = Physics.OverlapSphereNonAlloc(_headTransform.position, _viewDistance, _overlapBuffer, _playerMask);
            bool targetSpottedThisFrame = false;

            if (count > 0)
            {
                if (_overlapBuffer[0].TryGetComponent(out IVisibilityPointsProvider visibilityProvider))
                {
                    if (IsAnyPointVisible(visibilityProvider))
                    {
                        targetSpottedThisFrame = true;
                    }
                }
            }

            // Логика состояний
            if (targetSpottedThisFrame)
            {
                if (!_hasTarget) OnTargetSpotted();
            }
            else
            {
                if (_hasTarget) OnTargetLost();
            }
        }

        private bool IsAnyPointVisible(IVisibilityPointsProvider provider)
        {
            foreach (var point in provider.Points)
            {
                if (point == null) 
                    continue;

                Vector3 directionToPoint = point.position - _headTransform.position;
                float distanceToPoint = directionToPoint.magnitude;

                Vector3 localDir = _headTransform.InverseTransformDirection(directionToPoint);

                float horizontalAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
                if (Mathf.Abs(horizontalAngle) > _viewAngle / 2f) 
                    continue;

                float verticalAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
                if (Mathf.Abs(verticalAngle) > _viewHeightAngle / 2f) 
                    continue;

                if (!Physics.Raycast(_headTransform.position, directionToPoint.normalized, out RaycastHit hit, distanceToPoint, _obstacleMask))
                {
                    Debug.DrawLine(_headTransform.position, point.position, Color.green, 0.1f);
                    return true;
                }
                else
                {
                    Debug.DrawLine(_headTransform.position, hit.point, Color.red, 0.1f);
                }
            }
            return false;
        }

        private void OnTargetSpotted()
        {
            _hasTarget = true;
            StopAggroCoroutine();

            if (_follow != null) 
                _follow.enabled = true;
        }

        private void OnTargetLost()
        {
            _hasTarget = false;
            _aggroCoroutine = StartCoroutine(SwitchFollowOffAfterCooldown());
        }

        private IEnumerator SwitchFollowOffAfterCooldown()
        {
            yield return new WaitForSeconds(_cooldown);

            if (_follow != null) 
                _follow.enabled = false;

            Debug.Log("<color=orange>[AGGRO]</color> Цель потеряна. Возврат в режим ожидания.");
        }

        private void StopAggroCoroutine()
        {
            if (_aggroCoroutine != null)
            {
                StopCoroutine(_aggroCoroutine);
                _aggroCoroutine = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_headTransform == null) 
                return;

            PhysicsDebug.DrawViewSector(_headTransform, _viewAngle, _viewHeightAngle, _viewDistance, Color.cyan);

            if (Application.isPlaying && _hasTarget && _overlapBuffer[0] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_headTransform.position, _overlapBuffer[0].transform.position);
            }
        }
    }
}
