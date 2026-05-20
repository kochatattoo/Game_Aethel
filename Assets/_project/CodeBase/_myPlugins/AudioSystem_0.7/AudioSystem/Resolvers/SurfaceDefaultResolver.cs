using Infrastructure.AudioSystem.Components.MaterialConfigs;
using UnityEngine;

namespace Infrastructure.AudioSystem.Resolvers
{
    /// <summary>
    /// Дефолтный Resolver - для получения дефолтного конфига воспроизведения звука (определения поверхности)
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого ключа</typeparam>
    public class SurfaceDefaultResolver<T> : BaseSurfaceResolver<T>
    {
        /// <summary>
        /// Инициализирует резолвер с настройками дистанции и слоями коллизий.
        /// </summary>
        /// <param name="config">Конфигурация параметров каста и дефолтных значений.</param>
        public SurfaceDefaultResolver(BaseSurfaceResolverConfig<T> config) : base(config)
        { }

        public override bool TryResolve(RaycastHit[] raycastHits, ref T resolve)
        {
            LastDebugReason = "Raycast Hit Nothing";
            resolve = _config.DefaultSwitch;
            LastDetectedSurface = resolve;

            return true;
        }
    }
}