using CodeBase.Hero;
using System;
using UnityEngine;

namespace CodeBase.Components
{
    [Serializable]
    public class VisionSensor
    {
        [Header("Field of View")]
        [SerializeField, Range(0, 180)] private float _horizontalAngle = 150f;
        [SerializeField, Range(0, 180)] private float _verticalAngle = 45f;

        [Header("Collision Settings")]
        [SerializeField] private LayerMask _obstacleMask;

        private readonly Collider[] _buffer = new Collider[10]; // Буфер для результатов поиска

        public float HorizontalAngle => _horizontalAngle;
        public float VerticalAngle => _verticalAngle;

        /// <summary>
        /// Сканирует область и возвращает количество найденных видимых объектов.
        /// </summary>
        public int Scan(Transform head, float distance, LayerMask targetMask, out Collider[] visibleResults)
        {
            visibleResults = _buffer;
            int count = Physics.OverlapSphereNonAlloc(head.position, distance, _buffer, targetMask);
            int visibleCount = 0;

            for (int i = 0; i < count; i++)
            {
                if (IsTargetVisible(head, _buffer[i], distance))
                {
                    // Перемещаем видимый объект в начало буфера для удобства
                    _buffer[visibleCount] = _buffer[i];
                    visibleCount++;
                }
            }
            return visibleCount;
        }

        private bool IsTargetVisible(Transform head, Collider target, float distance)
        {
            if (target.TryGetComponent(out IVisibilityPointsProvider provider))
            {
                foreach (var point in provider.Points)
                {
                    if (point == null) 
                        continue;
                    if (CheckPoint(head, point.position, distance)) 
                        return true;
                }
            }
            else 
            {
                if (CheckPoint(head, target.bounds.center, distance)) 
                    return true;
            }

            return false;
        }

        private bool CheckPoint(Transform head, Vector3 targetPos, float distance)
        {
            Vector3 direction = targetPos - head.position;
            float distToTarget = direction.magnitude;

            if (distToTarget > distance) 
                return false;

            Vector3 localDir = head.InverseTransformDirection(direction);

            float hAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
            if (Mathf.Abs(hAngle) > _horizontalAngle / 2f) 
                return false;

            float vAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
            if (Mathf.Abs(vAngle) > _verticalAngle / 2f) 
                return false;

            return !Physics.Raycast(head.position, direction.normalized, distToTarget, _obstacleMask);
        }
    }
}
