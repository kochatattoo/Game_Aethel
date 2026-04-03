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

        public void Play(float impactScale)
        {
            if (_particleSystem == null)
            {
                Debug.LogError($"ParticleSystem is missing on {gameObject.name}!");
                return;
            }

            // Очищаем старые частицы, если они вдруг остались
            _particleSystem.Clear();

            // Опционально: применяем масштаб удара к размеру частиц
            var main = _particleSystem.main;
            // main.startSizeMultiplier = impactScale; 

            _particleSystem.Play(true); // true — проигрывать включая дочерние системы
        }

        public void Stop()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
