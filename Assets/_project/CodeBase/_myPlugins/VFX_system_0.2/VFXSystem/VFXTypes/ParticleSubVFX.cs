using UnityEngine;

namespace VFXSystem.VFXTypes
{
    /// <summary>
    /// Обертка для систем частиц (ParticleSystem).
    /// Автоматически вычисляет длительность на основе параметров основной системы.
    /// </summary>
    public class ParticleSubVFX : MonoBehaviour, ISubVFX
    {
        [SerializeField] 
        private ParticleSystem _particleSystem;
        public float Duration => _particleSystem.main.duration + _particleSystem.main.startLifetime.constantMax;

        public GameObject GameObject => gameObject;

        public void Play(float impactScale) => _particleSystem.Play();
        public void Stop() => _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
