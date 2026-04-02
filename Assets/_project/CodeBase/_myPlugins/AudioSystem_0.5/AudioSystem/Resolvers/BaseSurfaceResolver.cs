using Infrastructure.AudioSystem.Components.MaterialConfigs;
using UnityEngine;

namespace Infrastructure.AudioSystem.Resolvers
{
    public abstract class BaseSurfaceResolver<T>: ISurfaceResolver<T>
    {
        protected readonly LayerMask _layerMask;
        protected readonly BaseSurfaceResolverConfig<T> _config;
        protected readonly RaycastHit[] _raycastHit = new RaycastHit[10];

        public string LastDebugReason { get; protected set; }

        public T LastDetectedSurface { get; protected set; }

        /// <summary>
        /// Инициализирует резолвер с настройками дистанции и слоями коллизий.
        /// </summary>
        /// <param name="config">Конфигурация параметров каста и дефолтных значений.</param>
        public BaseSurfaceResolver(BaseSurfaceResolverConfig<T> config)
        {
            _config = config;
            _layerMask = config.CastSettings.LayerMask;
        }

        public T Resolve(RaycastHit[] raycastHits)
        {
            T result = _config.DefaultSwitch;
            if (TryResolve(raycastHits, ref result))
            {
                return result;
            }

            LastDebugReason = "Key not found or mapping missing, using Default";
            return LastDetectedSurface = _config.DefaultSwitch;
        }

        public abstract bool TryResolve(RaycastHit[] raycastHits, ref T resolve);

        /// <summary>
        /// Получчение самого ближнего Hit react
        /// </summary>
        /// <param name="raycastHits">Массив полученных Hit react'ов</param>
        /// <returns>Ближайший хит</returns>
        protected RaycastHit GetClosestRaycastHit(RaycastHit[] raycastHits)
        {
            var minDistance = float.MaxValue;
            RaycastHit hit = raycastHits[0];

            foreach (var raycastHit in raycastHits)
            {
                if (!raycastHit.collider)
                    continue;

                if (raycastHit.distance < minDistance)
                {
                    hit = raycastHit;
                    minDistance = raycastHit.distance;
                }
            }

            return hit;
        }

        /// <summary>
        /// Получение GameObject из Hit
        /// </summary>
        protected GameObject GetGameObject(RaycastHit hit) => 
            hit.collider.gameObject;

        /// <summary>
        /// Получение GameObject из самого ближнего Hit react
        /// </summary>
        protected GameObject GetClosestGameObject(RaycastHit[] raycastHits) => GetGameObject(GetClosestRaycastHit(raycastHits));
    }
}