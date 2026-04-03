using System.Collections.Generic;
using UnityEngine;
using VFXSystem.Components;
using VFXSystem.Resolver;

namespace VFXSystem.Sensors
{
    public static class VFXHitSensor
    {
        /// <summary>
        /// Определяет примерную точку касания и нормаль для Collider.
        /// Подходит для быстрых эффектов, где производительность важнее ювелирной точности.
        /// </summary>
        public static void GetContactData(Collider target, Vector3 origin, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            // 1. Находим ближайшую точку на поверхности коллайдера
            hitPoint = target.ClosestPoint(origin);
            hitNormal = Vector3.up;

            Vector3 direction = hitPoint - origin;

            // Если origin не совпадает с точкой касания, уточняем нормаль через Raycast
            if (direction != Vector3.zero)
            {
                float dist = direction.magnitude;
                Vector3 dirNormalized = direction / dist;

                // Слегка отступаем назад для надежности Raycast
                Ray ray = new Ray(origin - dirNormalized * 0.1f, dirNormalized);

                if (target.Raycast(ray, out RaycastHit hit, dist + 0.2f))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                }
                else
                {
                    // Если Raycast не попал (редкий случай для выпуклых сеток), 
                    // используем направление от центра как импровизированную нормаль
                    hitNormal = -dirNormalized;
                }
            }
        }

        /// <summary>
        /// Определяет точную точку попадания вдоль заданного вектора.
        /// Идеально для снарядов, лучей и точечных ударов.
        /// </summary>
        public static bool GetPreciseHit(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, out RaycastHit hit)
        {
            // Используем классический Raycast для максимальной точности
            // QueryTriggerInteraction.Ignore, чтобы не спавнить искры на триггерах зон
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
        public static bool GetThickHit(Vector3 origin, float radius, Vector3 direction, float distance, LayerMask layerMask, out RaycastHit hit)
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
        public static List<VFXPointData> GetMultiHitData(Collider[] hits, int count, Vector3 origin, float maxDistance)
        {
            var results = new List<VFXPointData>(count);

            for (int i = 0; i < count; i++)
            {
                Collider target = hits[i];
                if (target == null) continue;

                Vector3 hitPoint;
                Vector3 hitNormal;

                // 1. Пытаемся получить точную точку через Raycast к центру коллайдера
                Vector3 dirToTarget = (target.bounds.center - origin).normalized;

                if (target.Raycast(new Ray(origin - dirToTarget * 0.1f, dirToTarget), out RaycastHit hit, maxDistance + 0.5f))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                }
                else
                {
                    // 2. Фолбэк на аппроксимацию, если Raycast не прошел
                    GetContactData(target, origin, out hitPoint, out hitNormal);
                }

                // 3. Собираем данные, проверяя наличие хитбокса для типа материала
                if (target.TryGetComponent<IHitbox>(out IHitbox hitbox))
                {
                    results.Add(new VFXPointData(target.gameObject, hitPoint, hitNormal, 1f, hitbox.MaterialType));
                }
                else
                {
                    results.Add(new VFXPointData(target.gameObject, hitPoint, hitNormal));
                }
            }

            return results;
        }
    }
}
