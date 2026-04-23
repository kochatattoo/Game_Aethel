using Infrastructure.AudioSystem.Components.MaterialConfigs;
using Infrastructure.AudioSystem.Components.Materials;
using UnityEngine;

namespace Infrastructure.AudioSystem.Resolvers
{
    /// <summary>
    /// Резолвер для динамического определения аудио-поверхности.
    /// Использует поиск компонента реализующего <see cref="ISurfaceMaterial"/>.
    /// Вторичный поиск по тегу в <c>SurfaceConfig</c>.
    /// Анализирует физическое окружение и возвращает соответствующий ключ Wwise Switch.
    /// </summary>
    public class SurfaceComponentResolver<T>: BaseSurfaceResolver<T>
    {
        /// <summary>
        /// Инициализирует резолвер с настройками дистанции и слоями коллизий.
        /// </summary>
        /// <param name="config">Конфигурация параметров каста и дефолтных значений.</param>
        public SurfaceComponentResolver(BaseSurfaceResolverConfig<T> config): base(config) { }

        public override bool TryResolve(RaycastHit[] raycastHits, ref T resolve)
        {
            if (raycastHits.Length > 0)
            {
                RaycastHit hit = GetClosestRaycastHit(raycastHits);

                if (hit.collider.TryGetComponent<ISurfaceMaterial<T>>(out var material))
                {
                    LastDebugReason = "Found SurfaceMaterial Component";
                    resolve = material.SurfaceKey;
                    return true;
                }

                LastDebugReason = "SurfaceMaterial not found";
            }
            else
            {
                LastDebugReason = "Raycast Hit Nothing";
            }

            return false;
        }
    }
}