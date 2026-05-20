using System.Collections.Generic;
using UnityEngine;
using VFXSystem.Components;
using VFXSystem.Resolver;

namespace VFXSystem.Sensors
{
    public static class VFXHitSensor
    {
        private const float RayOffset = 0.1f;
        private const float SurfaceOffset = 0.06f;

        /// <summary>
        /// Оптимизированный расчет для группы целей. 
        /// Используем массив или Span, чтобы избежать аллокации List.
        /// </summary>
        public static int GetMultiHitDataNonAlloc(Collider[] hits,
            int count,
            Vector3 origin,
            float maxDistance,
            VFXPointData[] results)
        {
            int actualCount = 0;

            for (int i = 0; i < count; i++)
            {
                Collider target = hits[i];
                if (target == null) 
                    continue;

                GetSurfacePoint(target, origin, out var hitPoint, out var hitNormal, out var hitRotation);

                var materialType = target.TryGetComponent<IHitbox>(out var hitbox)
                    ? hitbox.MaterialType
                    : BaseTypes.MaterialType.None;

                results[actualCount++] = new VFXPointData(target.gameObject, hitPoint, hitNormal, hitRotation, 1f, materialType);
            }
            return actualCount;
        }

        /// <summary>
        /// Получение точек касания поверхности
        /// </summary>
        public static void GetSurfacePoint(Collider target,
          Vector3 origin,
          out Vector3 hitPoint,
          out Vector3 hitNormal,
          out Quaternion hitRotation)
        {
            Vector3 closest = target.ClosestPoint(origin);
            bool isInside = (closest - origin).sqrMagnitude < 0.00001f;

            if (isInside)
            {
                Vector3 center = target.bounds.center;
                Vector3 directionFromCenter = (origin - center).normalized;

                const float farDist = 2.0f;
                Ray ray = new Ray(origin + directionFromCenter * farDist, -directionFromCenter);

                if (target.Raycast(ray, out RaycastHit hit, farDist + 1f))
                {
                    hitPoint = hit.point - hit.normal * SurfaceOffset;
                    hitNormal = hit.normal;
                }
                else
                {
                    hitNormal = directionFromCenter;
                    hitPoint = closest - hitNormal * SurfaceOffset;
                }
            }
            else
            {
                Vector3 dirToTarget = (target.bounds.center - origin).normalized;
                Ray ray = new Ray(origin - dirToTarget * RayOffset, dirToTarget);

                if (target.Raycast(ray, out RaycastHit hit, 10f))
                {
                    hitPoint = hit.point - hit.normal * SurfaceOffset;
                    hitNormal = hit.normal;
                }
                else
                {
                    hitPoint = closest;
                    hitNormal = (origin - target.bounds.center).normalized;
                }
            }

            // РАСЧЕТ ПОВОРОТА:
            // Используем -hitNormal, чтобы Z-axis (Forward) декали смотрела В поверхность.
            // Vector3.up помогает избежать случайного вращения "волчком" вокруг оси нормали.
            hitRotation = Quaternion.LookRotation(hitNormal, Vector3.up);
        }

        /// <summary>
        /// Определяет примерную точку касания и нормаль для Collider.
        /// Подходит для быстрых эффектов, где производительность важнее ювелирной точности.
        /// </summary>
        public static void GetContactData(Collider target, 
            Vector3 origin, 
            out Vector3 hitPoint, 
            out Vector3 hitNormal, 
            out Quaternion hitRotation)
        {
            hitPoint = target.ClosestPoint(origin);
            hitNormal = Vector3.up;
            Vector3 direction = hitPoint - origin;

            if (direction != Vector3.zero)
            {
                float dist = direction.magnitude;
                Vector3 dirNormalized = direction / dist;

                Ray ray = new Ray(origin - dirNormalized * 0.1f, dirNormalized);

                if (target.Raycast(ray, out RaycastHit hit, dist + 0.2f))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                }
                else
                {
                    hitNormal = -dirNormalized;
                }
            }
            hitRotation = Quaternion.LookRotation(-hitNormal, Vector3.up);
        }

        /// <summary>
        /// Определяет точную точку попадания вдоль заданного вектора.
        /// Идеально для снарядов, лучей и точечных ударов.
        /// </summary>
        public static bool GetPreciseHit(Vector3 origin, 
            Vector3 direction, 
            float distance, 
            LayerMask layerMask, 
            out RaycastHit hit)
        {
            if (Physics.Raycast(origin, direction, out hit, distance, layerMask, QueryTriggerInteraction.Ignore))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Вариант для широких ударов (например, тяжелый молот или толстый снаряд).
        /// Использует SphereCast для имитации объема.
        /// </summary>
        public static bool GetThickHit(Vector3 origin, 
            float radius, 
            Vector3 direction, 
            float distance, 
            LayerMask layerMask, 
            out RaycastHit hit)
        {
            if (Physics.SphereCast(origin, radius, direction, out hit, distance, layerMask, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Массово вычисляет точки касания для набора коллайдеров.
        /// Идеально для AOE атак и широких взмахов.
        /// </summary>
        public static List<VFXPointData> GetMultiHitData(Collider[] hits, 
            int count, 
            Vector3 origin, 
            float maxDistance)
        {
            var results = new List<VFXPointData>(count);

            for (int i = 0; i < count; i++)
            {
                Collider target = hits[i];
                if (target == null) 
                    continue;

                Vector3 hitPoint;
                Vector3 hitNormal;
                Quaternion hitRotation;

                Vector3 dirToTarget = (target.bounds.center - origin).normalized;

                if (target.Raycast(new Ray(origin - dirToTarget * 0.1f, dirToTarget), out RaycastHit hit, maxDistance + 0.5f))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                    hitRotation = Quaternion.LookRotation(-hitNormal, Vector3.up);
                }
                else
                {
                    GetContactData(target, origin, out hitPoint, out hitNormal, out hitRotation);
                }

                if (target.TryGetComponent<IHitbox>(out IHitbox hitbox))
                {
                    results.Add(new VFXPointData(target.gameObject, hitPoint, hitNormal, hitRotation, 1f, hitbox.MaterialType));
                }
                else
                {
                    results.Add(new VFXPointData(target.gameObject, hitPoint, hitNormal, hitRotation));
                }
            }
            return results;
        }

        /// <summary>
        /// Если точка оказалась внутри коллайдера, выносит её на ближайшую поверхность.
        /// Полезно, если снаряд пролетел слишком глубоко за один кадр.
        /// </summary>
        public static void ResolveInternalPoint(Collider target, 
            Vector3 internalPoint, 
            out Vector3 surfacePoint, 
            out Vector3 surfaceNormal, 
            out Quaternion hitRotation)
        {

            if (Physics.ComputePenetration(
                null, internalPoint, Quaternion.identity, 
                target, target.transform.position, target.transform.rotation,
                out Vector3 direction, out float distance))
            {
                surfacePoint = internalPoint + (direction * (distance + 0.01f));
                surfaceNormal = direction;
            }
            else
            {
                GetContactData(target, internalPoint, out surfacePoint, out surfaceNormal, out hitRotation);
            }

            hitRotation = Quaternion.LookRotation(surfaceNormal, Vector3.up);
        }
    }
}
