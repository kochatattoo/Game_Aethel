using UniRx;
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
        private readonly BoolReactiveProperty _isPlaying = new BoolReactiveProperty(false);
        public float Duration => _particleSystem != null
            ?_particleSystem.main.duration + _particleSystem.main.startLifetime.constantMax 
            : 0f;

        public GameObject GameObject => gameObject;

        public BoolReactiveProperty IsPlayingObservable => _isPlaying;

        public void Play(float impactScale)
        {
            if (_particleSystem == null)
            {
                Debug.LogError($"ParticleSystem is missing on {gameObject.name}!");
                return;
            }

            _particleSystem.Clear();

            //var main = _particleSystem.main;
            //main.startSizeMultiplier = impactScale;

            _particleSystem.Play(true); // true — проигрывать включая дочерние системы
            _isPlaying.Value = true;

            Observable.Timer(System.TimeSpan.FromSeconds(Duration))
               .Subscribe(_ => _isPlaying.Value = false)
               .AddTo(this);
        }

        public void Stop()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                _isPlaying.Value = false;
            }
        }

        public void ResetVFX()
        {
            if (_particleSystem == null) 
                return;

            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particleSystem.Clear(true);
            _isPlaying.Value = false;
        }

        private void OnDestroy()
        {
            _isPlaying.Dispose();
        }
    }
}
