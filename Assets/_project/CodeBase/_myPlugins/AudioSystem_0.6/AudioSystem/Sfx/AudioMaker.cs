using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.Sensors;
using Infrastructure.AudioSystem.Parameters.DTO;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Domain.Character.Core.Sfx
{
    public class AudioMaker : MonoBehaviour, IAudioEnviromentMaker
    {
        // TODO: Расширить компонет
        private IAudioFacade _audioFacade;
       // private IMovementService _movementService;
        private CharacterController _controller;
        private EnviromentResolver _enviromentResolver;
        private PortalResolver _portalResolver;
        private NavMeshAgent _agent;
        public float CurrentSpeed => _agent.velocity.magnitude;

        public GameObject AudioMakerObject => this.gameObject;
        public EnviromentResolver EnviromentResolver => _enviromentResolver;
        public PortalResolver PortalResolver => _portalResolver;

        [Inject]
        private void Construct(IAudioFacade audioFacade)
        {
            _audioFacade = audioFacade;
            //_controller = personPhysics.CharacterController;
           // _movementService = movementService;
            _enviromentResolver = new(gameObject, _audioFacade);
            _portalResolver = new(this.transform);
        }

        public AuxSendData GetCurrentAuxSendData()
        {
            bool isInPortal = _portalResolver.IsInPortal;
            _enviromentResolver.IsInPortal = isInPortal;

            if (isInPortal)
                return _portalResolver.GetBlendedAuxSendData();

            var bus = _enviromentResolver.GetCurrentAuxBus();
            if (bus != null)
            {
                return new AuxSendData
                {
                    AuxBusA = bus,
                    VolumeA = 1f,
                    IsBlended = false
                };
            }
            return default;
        }
    }
}
