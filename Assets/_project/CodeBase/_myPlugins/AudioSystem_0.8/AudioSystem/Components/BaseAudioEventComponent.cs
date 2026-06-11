using AudioSystem.Banks;
using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Components.Occlusions;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Zones;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    /// <summary>
    /// Базовый класс для работы с компонентами в альтернативу классам от SDK Wwise
    /// </summary>
    [RequireComponent(typeof(AkGameObj))]
    public abstract class BaseAudioEventComponent: MonoBehaviour, IOcclusionEmitter
    {
        [Header("Wwise настройки")]
        [SerializeField]
        private AudioEventAsset _wwiseEvent;
        [SerializeField]
        protected bool _playOnStart = false;

        [Header("Environment Auto-Apply")]
        [SerializeField]
        private bool _autoEnvironment = false;

        [Header("Occlusion")]
        [SerializeField]
        private bool _enableOcclusion = false;

        [Header("Reposition")]
        [SerializeField]
        private bool _enableOverridePosition = true;
        private bool _positionOverridden = false;
        private Vector3 _originalPosition;
        private Vector3 _originalForward;
        private Vector3 _originalUp;

        protected IAudioFacade _audioFacade;

        private IBankLoader _bankLoader;
        private IAudioZoneRegistry _zoneRegistry;
        private IOcclusionService _occlusionService;
        private IDisposable _waitForBanks;

        public Transform EmitterTransform => transform;

        [Inject]
        private void Construct(IAudioFacade audioFacade,
            IBankLoader bankLoader,
            [InjectOptional] IAudioZoneRegistry zoneRegistry,
            [InjectOptional] IOcclusionService occlusionService)
        {
            _audioFacade = audioFacade;
            _bankLoader = bankLoader;
            _zoneRegistry = zoneRegistry;
            _occlusionService = occlusionService;
        }

        protected virtual void Start()
        {
            _originalPosition = transform.position;
            _originalForward = transform.forward;
            _originalUp = transform.up;

            _audioFacade.Register(gameObject);

            if (_autoEnvironment && _zoneRegistry != null)
            {
                if (_zoneRegistry.IsReady)
                    ApplyInitialEnvironment();
                else
                    _zoneRegistry.Ready
                        .Where(ready => ready)
                        .Take(1)
                        .Subscribe(OnZoneRegistryReady)
                        .AddTo(this);
            }

            if (_playOnStart)
            {
                if (_bankLoader.AreSceneBanksLoaded)
                    Play();
                else
                    _waitForBanks = _bankLoader.OnSceneBanksLoaded
                        .Subscribe(Play)
                        .AddTo(this);
            }

            if (_enableOcclusion && _occlusionService != null)
                _occlusionService.RegisterEmitter(this);
        }

        public virtual void Play()
        {
            _waitForBanks?.Dispose();
            _audioFacade.PostEvent(_wwiseEvent.WwiseEvent, gameObject);
        }

        public virtual void Stop()
        {
            _audioFacade.StopEvent(_wwiseEvent.WwiseEvent, gameObject);
        }

        public void OverridePosition(Vector3 newPosition)
        {
            if (_enableOverridePosition && !_positionOverridden)
            {
                _positionOverridden = true;
                // Сохраняем текущие, если они ещё не были сохранены
                _originalPosition = transform.position;
                _originalForward = transform.forward;
                _originalUp = transform.up;
            }
            _audioFacade.SetGameObjectPosition(gameObject, newPosition, transform.forward, transform.up);
        }

        public void RestoreOriginalPosition()
        {
            if (_enableOverridePosition && _positionOverridden)
            {
                _positionOverridden = false;
                _audioFacade.SetGameObjectPosition(gameObject, _originalPosition, _originalForward, _originalUp);
            }
        }

        protected virtual void OnDisable()
        {
            Stop();

            if (_enableOcclusion && _occlusionService != null)
                _occlusionService.UnregisterEmitter(this);
        }

        private void OnDestroy()
        {
            _audioFacade.UnRegister(gameObject);
        }

        private void OnZoneRegistryReady(bool _) => ApplyInitialEnvironment();

        private void ApplyInitialEnvironment()
        {
            Vector3 pos = transform.position;
            Collider[] hits = Physics.OverlapSphere(pos, 0.1f);
            IEnvironmentComponent best = null;
            int bestPriority = int.MinValue;

            foreach (var col in hits)
            {
                var env = col.GetComponent<EnvironmentAudioComponent>();
                if (env != null && env.AuxBus != null && env.Priority > bestPriority)
                {
                    best = env;
                    bestPriority = env.Priority;
                }

                var room = col.GetComponent<RoomComponent>();
                if (room != null && room.AuxBus != null && room.Priority > bestPriority)
                {
                    best = room;
                    bestPriority = room.Priority;
                }
            }

            if (best != null)
                _audioFacade.SetGameObjectAuxSend(gameObject, best.AuxBus);
        }
    }
}
