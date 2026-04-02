using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Components.Interfaces;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Domain.Character.Core.Sfx
{
    public class AudioMaker : MonoBehaviour, IAudioMaker
    {
        // TODO: Расширить компонет
        private IAudioFacade _audioFacade;
        private CharacterController _controller;
        private NavMeshAgent _agent;
        public float CurrentSpeed => _agent.velocity.magnitude;

        [Inject]
        private void Construct(IAudioFacade audioFacade)
        {
            _audioFacade = audioFacade;

        }
    }
}
