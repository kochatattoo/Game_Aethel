using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Resolvers;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Sensors
{
    /// <summary>
    /// Базовый компонент для обработки шагов, связывающий физику окружения с аудио-системой.
    /// Содержит общую логику кастинга лучей (Raycast), визуализации дебага и последовательного 
    /// опроса цепочки резолверов для определения типа поверхности.
    /// </summary>
    /// <typeparam name="T">Тип ключа переключателя (Switch) в аудио-движке.</typeparam>
    public class FootstepResolver<T> : IFootstepResolver<T>
    {
        protected readonly FootstepResolverData<T> _data;
        protected readonly PhysicSurfaceRaycast<T> _physicSurfaceRaycast;
        protected readonly float _minVelocity;

        protected readonly IPhysicAudioMaker _physicAudioMaker;

        private readonly List<ISurfaceResolver<T>> _resolvers;   //TODO: Конфиг или что то чем можно управлять чтоб заполнять
                                                                 // Или реализовать фабрику для заполнения
        private ISurfaceResolver<T> _resolver;

        /// <summary>
        /// Конструктор инициализирует зависимости и формирует приоритетный список резолверов: 
        /// Компонент -> Материал -> Террейн -> Тег -> Дефолт.
        /// </summary>
        public FootstepResolver(
                FootstepResolverData<T> data,
                IPhysicAudioMaker personPhysics)
        {
            _data = data;
            _physicSurfaceRaycast = new(data.Config.CastSettings);
            _minVelocity = data.Config.CastSettings.Min_Velocity;
            _physicAudioMaker = personPhysics;

            _resolvers = new()
            {
                new SurfaceComponentResolver<T>(_data.Config) ,
                new SurfaceMaterialResolver<T>(_data.Config) ,
                new SurfaceTerrainResolver<T>(_data.Config) ,
                new SurfaceTagResolver<T>(_data.Config) ,
                new SurfaceDefaultResolver<T>(_data.Config)
            };
        }

        [CanBeNull]
        public T GetResolveredKey(int footId, out Transform targetFoot)
        {
            targetFoot = (footId == 0) ? _data.LeftFoot : _data.RightFoot;

            if (_physicAudioMaker != null && (!_physicAudioMaker.IsGrounded || _physicAudioMaker.CurrentSpeed < _minVelocity))
                return default(T);

            if (targetFoot == null)
                return default(T);

            _physicSurfaceRaycast.Hit(targetFoot.position);

            T surfaceMaterial = _data.Config.DefaultSwitch;
            Resolve(_physicSurfaceRaycast.Raycasts, ref surfaceMaterial);

            return surfaceMaterial;
        }

        /// <summary>
        /// Последовательно опрашивает зарегистрированные резолверы. 
        /// Возвращает значение первого резолвера, который успешно идентифицировал поверхность.
        /// </summary>
        /// <param name="raycastHits">Результаты физического сканирования под ногой.</param>
        /// <param name="switchValue">Контейнер для записи найденного значения.</param>
        /// <returns>Найденный ключ переключателя поверхности.</returns>
        private void Resolve(RaycastHit[] raycastHits, ref T switchValue)
        {
            foreach (var resolvers in _resolvers)
            {
                if (resolvers.TryResolve(raycastHits, ref switchValue))
                {
                    _resolver = resolvers;
                    break;
                }
            }
        }

        public void OnDrawGizmosSelected()
        {
            if (!_data.LeftFoot || !_data.RightFoot || _resolver == null)
                return;

            string debugInfo = $"Surface: {_resolver.LastDetectedSurface}\nReason: {_resolver.LastDebugReason}";
#if UNITY_EDITOR
            UnityEditor.Handles.Label(_data.Root.position + Vector3.up * 2f, debugInfo);
#endif
            Gizmos.color = Color.purple;
            DrawFootGizmo(_data.LeftFoot.position);
            DrawFootGizmo(_data.RightFoot.position);
        }

        private void DrawFootGizmo(Vector3 pos)
        {
            Vector3 origin = pos + Vector3.up * _data.Config.CastSettings.VerticalOffset;
            Vector3 end = origin + Vector3.down * _data.Config.CastSettings.CastDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, _data.Config.CastSettings.CastRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(end, _data.Config.CastSettings.CastRadius);
        }
    }
}
