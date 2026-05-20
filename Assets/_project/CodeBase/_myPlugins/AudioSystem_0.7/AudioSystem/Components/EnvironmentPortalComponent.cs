using Infrastructure.AudioSystem.Components.Interfaces;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    [RequireComponent(typeof(Collider))]
    public class EnvironmentPortalComponent : MonoBehaviour
    {
        [SerializeField, Tooltip("Направление вдоль которого происходит расчет внутри портала")] 
        private Vector3 _axis = Vector3.right;
        [SerializeField, Tooltip("Слой на котором располагаются EnvironmentComponents колайдеры")]
        private LayerMask _environmentLayerMask = ~0;
        [SerializeField]
        private Collider _collider;

        private EnvironmentAudioComponent _environmentA;
        private EnvironmentAudioComponent _environmentB;
        private IAudioFacade _audioFacade;

        private readonly CompositeDisposable _disposables = new();
        private readonly List<IAudioEnviromentMaker> _trackedMakers = new List<IAudioEnviromentMaker>();

        public Vector3 Axis => _axis;

        [Inject]
        private void Construct(IAudioFacade audioFacade)
        {
            _audioFacade = audioFacade;
        }

        private void Awake()
        {
            if(_collider == null)
                _collider = GetComponent<Collider>();

            _collider.isTrigger = true;
        }

        private void Start() => 
            FindOverlappingEnvironments();

        private void FindOverlappingEnvironments()
        {
            if (_collider == null)
                return;

            var bounds = _collider.bounds;
            var results = Physics.OverlapBox(bounds.center, bounds.extents, transform.rotation, _environmentLayerMask);

            var foundEnvironments = new List<EnvironmentAudioComponent>();
            foreach (var col in results)
            {
                if (col.TryGetComponent<EnvironmentAudioComponent>(out var env) &&
                    !foundEnvironments.Contains(env) &&
                    env.AuxBus != null)
                {
                    foundEnvironments.Add(env);
                }
            }

            if (foundEnvironments.Count == 2)
            {
                _environmentA = foundEnvironments[0];
                _environmentB = foundEnvironments[1];
                Debug.Log($"[Portal] Connected '{_environmentA.name}' and '{_environmentB.name}'.");
            }
            else
            {
                Debug.LogWarning($"[Portal] Expected 2 valid environments, found {foundEnvironments.Count}. Disabling.", this);
                enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled)
                return;

            if (other.TryGetComponent<IAudioEnviromentMaker>(out var maker))
            {
                if (!_trackedMakers.Contains(maker))
                {
                    _trackedMakers.Add(maker);

                    maker.PortalResolver.EnterPortal(this, _environmentA, _environmentB);

                    var makerTransform = maker.AudioMakerObject.transform;
                    Observable.EveryUpdate()
                              .Where(_ => makerTransform != null && makerTransform.hasChanged)
                              .TakeWhile(_ => maker != null && (maker as MonoBehaviour) != null)
                              .Subscribe(_ =>
                              {
                                    ApplyBlendedAuxSendsToMaker(maker);
                                    makerTransform.hasChanged = false;
                              })
                              .AddTo(_disposables);

                    ApplyBlendedAuxSendsToMaker(maker);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IAudioEnviromentMaker>(out var maker))
            {
                if (!_trackedMakers.Remove(maker))
                    return;

                maker.PortalResolver.ExitPortal(this);
                // Сбросить на обычное окружение из EnviromentResolver
                maker.EnviromentResolver.UpdateAuxSend();
                RebuildSubscriptions();
            }
        }

        public float CalculateDistance(Vector3 position)
        {
            return Vector3.Distance(_collider.ClosestPoint(position), position);
        }

        public float CalculateWeight(Vector3 position)
        {
            var localPos = transform.InverseTransformPoint(position);
            var proj = Vector3.Dot(localPos, _axis.normalized);
            var localCenter = transform.InverseTransformPoint(_collider.bounds.center);
            var centerProj = Vector3.Dot(localCenter, _axis.normalized);
            var half = Vector3.Dot(_collider.bounds.extents, _axis.normalized);
            var min = centerProj - half;
            var max = centerProj + half;

            return Mathf.InverseLerp(min, max, proj);
        }

        private void ApplyBlendedAuxSendsToMaker(IAudioEnviromentMaker maker)
        {
            var data = maker.PortalResolver.GetBlendedAuxSendData();
            if (data.IsBlended)
            {
                _audioFacade.SetBlendedAuxSends(maker.AudioMakerObject, data.AuxBusA, data.VolumeA, data.AuxBusB, data.VolumeB);
            }
        }

        private void RebuildSubscriptions()
        {
            _disposables.Clear();
            _trackedMakers.RemoveAll(m => m == null || (m as MonoBehaviour) == null);
            foreach (var maker in _trackedMakers)
            {
                var makerTransform = maker.AudioMakerObject.transform;
                Observable.EveryUpdate()
                    .Where(_ => makerTransform.hasChanged)
                    .Subscribe(_ =>
                    {
                        ApplyBlendedAuxSendsToMaker(maker);
                        makerTransform.hasChanged = false;
                    })
                    .AddTo(_disposables);
            }
        }

        private void OnDestroy()
        {
            foreach (var maker in _trackedMakers)
            {
                if (maker != null)
                {
                    maker.PortalResolver.ExitPortal(this);
                    maker.EnviromentResolver?.UpdateAuxSend();
                }
            }
            _trackedMakers.Clear();
            _disposables.Dispose();
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (_collider == null) 
                _collider = GetComponent<Collider>();

            if (_collider == null) 
                return;

            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            var center = Vector3.zero;
            var size = Vector3.one;
            if (_collider is BoxCollider box)
            {
                center = box.center;
                size = box.size;
            }
            else if (_collider is SphereCollider sphere)
            {
                center = sphere.center;
                size = Vector3.one * sphere.radius * 2;
            }
            Gizmos.DrawWireCube(center, size);

            // Отрисовка оси
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(center - _axis.normalized * size.magnitude * 0.5f, _axis.normalized * size.magnitude);
        }
#endif
    }
}
