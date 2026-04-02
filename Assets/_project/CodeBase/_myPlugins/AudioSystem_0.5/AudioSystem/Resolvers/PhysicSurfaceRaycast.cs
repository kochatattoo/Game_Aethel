using Infrastructure.AudioSystem.Components.MaterialConfigs;
using UnityEngine;

namespace Infrastructure.AudioSystem.Resolvers
{
    /// <summary>
    /// Физический обработчик полученных данных при Raycast в поверхность земли
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого ключа</typeparam>
    public class PhysicSurfaceRaycast<T>
    {
        private readonly CastSettings _config;
        private readonly RaycastHit[] _raycastHit;

        /// <summary>
        /// Масив полученных Hit react'ов
        /// </summary>
        public RaycastHit[] Raycasts => _raycastHit; 

        public PhysicSurfaceRaycast(CastSettings castSettings)
        {
            _config = castSettings;
            _raycastHit = new RaycastHit[_config.Buffer];
        }

        /// <summary>
        /// Сделать SphereCast для заполнения массива
        /// </summary>
        /// <param name="footPosition">Исходная точка SphereCast</param>
        public void Hit(Vector3 footPosition)
        {
            Vector3 origin = footPosition + Vector3.up * _config.VerticalOffset;

            for (var i = 0; i < _config.Buffer; i++)
            {
                _raycastHit[i] = default;
            }

            int hitCount = Physics.SphereCastNonAlloc(origin, _config.CastRadius, Vector3.down, _raycastHit, _config.CastDistance, _config.LayerMask);

            Debug.Log("Get Hit NonAlloc count: " + hitCount);
        }

        /// <summary>
        /// Поулчение количества соприкосновений
        /// </summary>
        /// <param name="footPosition">Исходная точка SphereCast</param>
        /// <returns>Количество полученных Hit react'ов</returns>
        public int GetHitCount(Vector3 footPosition)
        {
            //TODO: На данный ммоент не используется -> обращаюсь к Raycasts.lenght
            Vector3 origin = footPosition + Vector3.up * _config.VerticalOffset;

            for (var i = 0; i < _config.Buffer; i++)
            {
                _raycastHit[i] = default;
            }

            int hitCount = Physics.SphereCastNonAlloc(origin, _config.CastRadius, Vector3.down, _raycastHit, _config.CastDistance, _config.LayerMask);

            Debug.Log("Get Hit NonAlloc count: " + hitCount);

            return hitCount;
        }
    }
}