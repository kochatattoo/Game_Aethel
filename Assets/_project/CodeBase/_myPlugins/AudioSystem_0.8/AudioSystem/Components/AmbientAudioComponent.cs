using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Components.Occlusions;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    /// <summary>
    /// Пользовательский класс - альтернатива AkAmbiet
    /// </summary>
    public class AmbientAudioComponent : BaseAudioEventComponent
    {

        protected override void Start() => base.Start(); // Выполнит Play(), если _playOnStart == true

        //найти применение
        public void StopAmbient()
        {
            Stop();
        }
    }
}
