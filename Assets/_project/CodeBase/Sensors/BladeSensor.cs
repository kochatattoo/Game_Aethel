using CodeBase.Configs;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Sensors
{
    public class BladeSensor : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private AttackConfig _attackConfig;

        private bool _isActive;
        private readonly Collider[] _hits = new Collider[10];
        private readonly HashSet<Collider> _hitColliders = new(); // Чтобы не бить одного и того же врага дважды за один взмах
        private System.Action<Collider, Vector3> _onHitCallback;

        public void StartSensing(System.Action<Collider, Vector3> onHit)
        {
            _isActive = true;
            _hitColliders.Clear();
            _onHitCallback = onHit;
        }

        public void StopSensing() => _isActive = false;

        private void Update()
        {
            if (!_isActive) 
                return;

            // Используем SphereCast или просто проверку дистанции от кончика меча
            // Но лучше всего - проверять положение меча в этом кадре
            // Vector3 currentPos = transform.position;

            Vector3 point0 = transform.TransformPoint(_attackConfig.Offset);
            Vector3 point1 = transform.TransformPoint(_attackConfig.Offset + Vector3.up * _attackConfig.BladeLength);

            // Здесь можно использовать твой существующий слой Hittable
            int hits = Physics.OverlapCapsuleNonAlloc(point0, point1, 0.5f, _hits, _attackConfig.LayerName);

            for (int i = 0; i < hits; i++)
            {
                Collider hit = _hits[i];
                if (hit != null && !_hitColliders.Contains(hit))
                {
                    _hitColliders.Add(hit);
                    // Передаем точку попадания как ближайшую точку на мече
                    _onHitCallback?.Invoke(hit, point1);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!_attackConfig.DrawGizmos) 
                return;

            Gizmos.color = _isActive ? _attackConfig.GizmoColor : new Color(_attackConfig.GizmoColor.r, _attackConfig.GizmoColor.g, _attackConfig.GizmoColor.b, 0.2f);

            // Рисуем линию-ось лезвия
            Vector3 p0 = transform.TransformPoint(_attackConfig.Offset);
            Vector3 p1 = transform.TransformPoint(_attackConfig.Offset + Vector3.up * _attackConfig.BladeLength);
            Gizmos.DrawLine(p0, p1);

            // Рисуем упрощенный "цилиндр" через сферу на концах и линии
            DrawWireCapsule(p0, p1, _attackConfig.Radius);
        }

        // Вспомогательный метод для отрисовки капсулы в Gizmos
        private void DrawWireCapsule(Vector3 p0, Vector3 p1, float radius)
        {
            Gizmos.DrawWireSphere(p0, radius);
            Gizmos.DrawWireSphere(p1, radius);
            Gizmos.DrawLine(p0 + transform.right * radius, p1 + transform.right * radius);
            Gizmos.DrawLine(p0 - transform.right * radius, p1 - transform.right * radius);
            Gizmos.DrawLine(p0 + transform.forward * radius, p1 + transform.forward * radius);
            Gizmos.DrawLine(p0 - transform.forward * radius, p1 - transform.forward * radius);
        }
    }
}
