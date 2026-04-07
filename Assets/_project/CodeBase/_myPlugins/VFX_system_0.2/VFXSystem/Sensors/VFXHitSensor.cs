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
        public static void GetContactData(Collider target, Vector3 origin, out Vector3 hitPoint, out Vector3 hitNormal, out Quaternion hitRotation)
        {
            // Находим ближайшую точку на поверхности коллайдера
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

            hitRotation = Quaternion.LookRotation(-hitNormal, Vector3.up);
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
                Quaternion hitRotation;

                // 1. Пытаемся получить точную точку через Raycast к центру коллайдера
                Vector3 dirToTarget = (target.bounds.center - origin).normalized;

                if (target.Raycast(new Ray(origin - dirToTarget * 0.1f, dirToTarget), out RaycastHit hit, maxDistance + 0.5f))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                    hitRotation = Quaternion.LookRotation(-hitNormal, Vector3.up);
                }
                else
                {
                    // 2. Фолбэк на аппроксимацию, если Raycast не прошел
                    GetContactData(target, origin, out hitPoint, out hitNormal, out hitRotation);
                }

                // 3. Собираем данные, проверяя наличие хитбокса для типа материала
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
        public static void ResolveInternalPoint(Collider target, Vector3 internalPoint, out Vector3 surfacePoint, out Vector3 surfaceNormal, out Quaternion hitRotation)
        {
            surfacePoint = internalPoint;
            surfaceNormal = Vector3.up;

            // ComputePenetration вычисляет направление и дистанцию для выхода из коллизии
            if (Physics.ComputePenetration(
                null, internalPoint, Quaternion.identity, // Наша гипотетическая точка (без коллайдера)
                target, target.transform.position, target.transform.rotation,
                out Vector3 direction, out float distance))
            {
                // Выталкиваем точку на поверхность + небольшой отступ, чтобы частицы не клипались
                surfacePoint = internalPoint + (direction * (distance + 0.01f));
                surfaceNormal = direction;
            }
            else
            {
                // Если мы не "внутри", используем ваш стандартный метод уточнения
                GetContactData(target, internalPoint, out surfacePoint, out surfaceNormal, out hitRotation);
            }

            hitRotation = Quaternion.LookRotation(surfaceNormal, Vector3.up);
        }

        public static void GetSurfacePoint(Collider target, Vector3 origin, out Vector3 hitPoint, out Vector3 hitNormal, out Quaternion hitRotation)
        {
            // 1. Находим ближайшую точку на поверхности (или внутри)
            Vector3 closest = target.ClosestPoint(origin);

            float offset = 0.06f;

            // Проверяем, находится ли origin внутри (дистанция почти ноль)
            if (Vector3.SqrMagnitude(closest - origin) < 0.0001f)
            {
                // МЫ ВНУТРИ: Используем направление от центра коллайдера, чтобы "вытолкнуть" точку
                Vector3 center = target.bounds.center;
                Vector3 directionFromCenter = (origin - center).normalized;

                // Делаем Raycast ИЗВНЕ в сторону центра, чтобы найти точную точку входа
                float farDist = 2.0f; // Достаточно, чтобы выйти за пределы большинства моделей
                Ray ray = new Ray(origin + directionFromCenter * farDist, -directionFromCenter);

                if (target.Raycast(ray, out RaycastHit hit, farDist + 1f))
                {
                    hitPoint = hit.point - hit.normal * offset; // Зансим на 1см внутрь
                    hitNormal = hit.normal;
                }
                else
                {
                    // Фолбэк, если Raycast не сработал
                    hitNormal = directionFromCenter;
                    hitPoint = closest - hitNormal * offset;
                }
            }
            else
            {
                // МЫ СНАРУЖИ: Просто пускаем луч к центру для точности
                Vector3 dirToTarget = (target.bounds.center - origin).normalized;
                Ray ray = new Ray(origin - dirToTarget * 0.1f, dirToTarget);

                if (target.Raycast(ray, out RaycastHit hit, 10f))
                {
                    hitPoint = hit.point - hit.normal * offset;
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
    }
}
