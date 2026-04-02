using UnityEngine;

namespace VFXSystem.Resolver
{
    public class InteractResolver
    {
        /// <summary>
        /// Выполняет физический каст и возвращает данные о поверхности
        /// </summary>
        public static InteractionResult CastRay(Vector3 origin, Vector3 direction, float distance, LayerMask mask)
        {
            InteractionResult result = new InteractionResult();

            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, mask))
            {
                result.IsHit = true;
                result.Point = hit.point;
                result.Normal = hit.normal;
                result.HitObject = hit.collider.gameObject;

                // Определяем материал (через тег, PhysicMaterial или кастомный скрипт)
                result.MaterialTag = GetMaterialTag(hit);
            }

            return result;
        }

        private static string GetMaterialTag(RaycastHit hit)
        {
            // Вариант А: Через PhysicMaterial (наиболее производительно)
            if (hit.collider.sharedMaterial != null)
            {
                return hit.collider.sharedMaterial.name;
            }

            // Вариант Б: Через тег объекта
            return hit.collider.tag;
        }
    }

    public struct InteractionResult
    {
        public bool IsHit;
        public Vector3 Point;
        public Vector3 Normal;
        public string MaterialTag; // Или перечисление/физический материал
        public GameObject HitObject;
    }
}
