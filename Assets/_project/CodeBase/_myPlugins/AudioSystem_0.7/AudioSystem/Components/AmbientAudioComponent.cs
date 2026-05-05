using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Components.Occlusions;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    /// <summary>
    /// Пользовательский класс - альтернатива AkAmbiet
    /// </summary>
    public class AmbientAudioComponent : BaseAudioEventComponent, IOcclusionEmitter
    {
        public Transform EmitterTransform => this.transform;
        private IOcclusionService _occlusionService;

        [Inject]
        private void Construct(IOcclusionService occlusionService)
        {
            _occlusionService = occlusionService;
        }

        protected override void Start()
        {
            _occlusionService.RegisterEmitter(this);
            base.Start(); // Выполнит Play(), если _playOnStart == true

        }

        //найти применение
        public void StopAmbient()
        {
            _occlusionService.UnregisterEmitter(this);
            Stop(); 
        }
    }
}
