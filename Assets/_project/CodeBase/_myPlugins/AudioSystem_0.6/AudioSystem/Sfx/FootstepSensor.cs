using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Components.MaterialConfigs;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Resolvers;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Domain.Character.Core.Sfx
{
    /// <summary>
    /// Сенсор шагов, интегрированный с системой анимаций и физическим резолвером поверхностей.
    /// Отвечает за логику определения материала под ногами и запуск звуковых ивентов шага.
    /// </summary>
    [Obsolete("Используем FootstepMaker с обновленной логикой работы Surface Resolvers")]
    public class FootstepSensor : MonoBehaviour
    {
        [Header("Bones References")]
        [SerializeField]
        private Transform _leftFoot;
        [SerializeField]
        private Transform _rightFoot;

        [Header("Settings")]
        [SerializeField]
        private SurfaceWwiseSwitchResolverConfig _config;
        [SerializeField]
        private float _minVelocity = 0.1f;

        private IAudioFacade _audioFacade;
        //private IMovementService _movementService;
        private CharacterController _controller;

        private List<ISurfaceResolver<AK.Wwise.Switch>> _resolvers;   //TODO: Конфиг или что то чем можно управлять чтоб заполнять
        private ISurfaceResolver<AK.Wwise.Switch> _resolver;
        private PhysicSurfaceRaycast<AK.Wwise.Switch> _raycast;

        [Inject]
        private void Construct(IAudioFacade audioFacade)
        {
            _audioFacade = audioFacade;
            //_isGrounded = personPhysics.CharacterController;
            //_movementService = movementService; 
            _raycast = new(_config.CastSettings);

            _resolvers = new()
            {
                new SurfaceComponentResolver<AK.Wwise.Switch>(_config) ,
                new SurfaceMaterialResolver<AK.Wwise.Switch>(_config) , 
                new SurfaceTerrainResolver<AK.Wwise.Switch>(_config) ,
                new SurfaceTagResolver<AK.Wwise.Switch>(_config) ,
                new SurfaceDefaultResolver<AK.Wwise.Switch>(_config) 
            };
        }

        /// <summary>
        /// Основной метод-обработчик для Animation Events. 
        /// Выполняет физическую проверку поверхности и инициирует воспроизведение звука.
        /// </summary>
        /// /// <param name="evt">Событие анимации. intParameter: 0 - левая нога, 1 - правая.</param>
        public void PlayFootstep(AnimationEvent evt)
        {
            if (_audioFacade == null) 
                return;

           // if (_isGrounded != null && (!_isGrounded.isGrounded || _movementService.CurrentSpeed01 < _minVelocity)) 
            //   return;

            int footId = evt.intParameter;
            Transform targetFoot = (footId == 0) ? _leftFoot : _rightFoot;

            if (targetFoot == null) 
                return;

            _raycast.Hit(targetFoot.position);

            AK.Wwise.Switch surfaceMaterial = _config.DefaultSwitch;
            Resolve(_raycast.Raycasts, ref surfaceMaterial);

            Debug.Log($"Surface Material: {surfaceMaterial}");

            if (evt.objectReferenceParameter is AudioEventAsset asset)
            {
                _audioFacade.PlayOneShot(asset, targetFoot.position)
                    .WithSwitch(surfaceMaterial)
                    .Play();
            }
        }

        private void Resolve(RaycastHit[] raycastHits, ref AK.Wwise.Switch switchValue)
        {
            foreach(var resolvers in _resolvers)
            {
                if (resolvers.TryResolve(raycastHits, ref switchValue))
                { 
                    _resolver = resolvers;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_leftFoot || !_rightFoot || _resolver == null) 
                return;

            string debugInfo = $"Surface: {_resolver.LastDetectedSurface}\nReason: {_resolver.LastDebugReason}";
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, debugInfo);
#endif
            Gizmos.color = Color.purple;
            DrawFootGizmo(_leftFoot.position);
            DrawFootGizmo(_rightFoot.position);
        }

        private void DrawFootGizmo(Vector3 pos)
        {
            Vector3 origin = pos + Vector3.up * _config.CastSettings.VerticalOffset;
            Vector3 end = origin + Vector3.down * _config.CastSettings.CastDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, _config.CastSettings.CastRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, end); 
            Gizmos.DrawWireSphere(end, _config.CastSettings.CastRadius); 
        }
    }
}
