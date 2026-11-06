using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.Sensors;
using Infrastructure.AudioSystem.Parameters.DTO;
using Infrastructure.AudioSystem.Zones;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Domain.Character.Core.Sfx
{
    public class AudioMaker : MonoBehaviour, IAudioEnviromentMaker
    {
        // TODO: Расширить компонет
        private IAudioFacade _audioFacade;
        private EnviromentResolver _enviromentResolver;
        private PortalResolver _portalResolver;
        private IDisposable _initSubscription;

        [SerializeField]
        private NavMeshAgent _agent;
        public float CurrentSpeed => _agent.velocity.magnitude;

        public GameObject AudioMakerObject => this.gameObject;
        public EnviromentResolver EnviromentResolver => _enviromentResolver;
        public PortalResolver PortalResolver => _portalResolver;

        [Inject]
        public void Construct(IAudioFacade audioFacade, IAudioZoneRegistry zoneRegistry)
        {
            _audioFacade = audioFacade;
            _enviromentResolver = new(gameObject, _audioFacade, zoneRegistry);
            _portalResolver = new(this.transform);

            _initSubscription = Observable.NextFrame()
                .Subscribe(EnviromentInitialize)
                .AddTo(this);
        }

        public AuxSendData GetCurrentAuxSendData()
        {
            bool isInPortal = _portalResolver.IsInPortal;
            _enviromentResolver.IsInPortal = isInPortal;

            if (isInPortal)
                return _portalResolver.GetBlendedAuxSendData();

            return _enviromentResolver.GetCurrentAuxSendData();
        }

        public void EnviromentInitialize(Unit _)
        {
            _enviromentResolver.InitializeZones();
            _initSubscription?.Dispose();
        }
    }
}
