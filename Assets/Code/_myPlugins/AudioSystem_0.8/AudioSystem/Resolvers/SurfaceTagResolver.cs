using Infrastructure.AudioSystem.Components.MaterialConfigs;
using UnityEngine;

namespace Infrastructure.AudioSystem.Resolvers
{
    /// <summary>
    /// Резолвер для динамического определения аудио-поверхности.
    /// Использует поиск по тегу в <c>SurfaceConfig</c>.
    /// Анализирует физическое окружение и возвращает соответствующий ключ Wwise Switch.
    /// </summary>
    public class SurfaceTagResolver<T> : BaseSurfaceResolver<T>
    {

        /// <summary>
        /// Инициализирует резолвер с настройками дистанции и слоями коллизий.
        /// </summary>
        /// <param name="config">Конфигурация параметров каста и дефолтных значений.</param>
        public SurfaceTagResolver(BaseSurfaceResolverConfig<T> config) : base(config) 
        {
        }

        public override bool TryResolve(RaycastHit[] raycastHits, ref T resolve)
        {
            if (raycastHits.Length > 0)
            {
                RaycastHit hit = GetClosestRaycastHit(raycastHits);

                if(hit.collider == null)
                    return false;

                foreach (var mapping in _config.GetTagMappings())
                {
                    if (hit.collider.CompareTag(mapping.Tag))
                    {
                        LastDebugReason = $"Found Tag Mapping: {mapping.Tag}";
                        resolve = mapping.Key;
                        LastDetectedSurface = resolve;

                        return true;
                    }
                }
                LastDebugReason = "SurfaceMaterial not found, using Default";
            }
            else
            {
                LastDebugReason = "Raycast Hit Nothing";
            }
            return false;
        }
    }
}