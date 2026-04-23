using Infrastructure.AudioSystem.Components.MaterialConfigs;
using Infrastructure.AudioSystem.Parameters;
using UnityEngine;

namespace Infrastructure.AudioSystem.Resolvers
{
    /// <summary>
    /// Резолвер для динамического определения аудио-поверхности.
    /// Приорететный поиск по Материалу на объекте.
    /// Вторичный поиск компонента реализующего <see cref="ISurfaceMaterial"/>.
    /// Последний поиск по тегу в <c>SurfaceConfig</c>.
    /// Анализирует физическое окружение и возвращает соответствующий ключ Wwise Switch.
    /// </summary>
    public class SurfaceMaterialResolver<T> : BaseSurfaceResolver<T>
    {
        private readonly WwiseSwitchMaping<T> _wwiseSwitchMaping;

        /// <summary>
        /// Инициализирует резолвер с настройками дистанции и слоями коллизий.
        /// </summary>
        /// <param name="config">Конфигурация параметров каста и дефолтных значений.</param>
        public SurfaceMaterialResolver(BaseSurfaceResolverConfig<T> config) : base(config) 
        {
            _wwiseSwitchMaping = new(_config.WwiseToMaterial);
        }

        public override bool TryResolve(RaycastHit[] raycastHits, ref T resolve)
        {
            if (raycastHits.Length > 0)
            {
                GameObject hitObject = GetClosestGameObject(raycastHits);

                var renderer = hitObject.GetComponentInChildren<MeshRenderer>();

                if (renderer == null)
                    renderer = hitObject.GetComponentInParent<MeshRenderer>();

                if (renderer != null)
                {
                    Material visualMat = renderer.sharedMaterial; // Подумать над кешированием и обращению если матриал тот же (хотя у нас словарь 0(1))

                    if (visualMat != null && _wwiseSwitchMaping.Map.TryGetValue(visualMat, out T switchValue))
                    {
                        LastDebugReason = $"Found Visual Material: {visualMat.name}";

                        resolve = switchValue;
                        LastDetectedSurface = resolve;

                        return true;
                    }
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