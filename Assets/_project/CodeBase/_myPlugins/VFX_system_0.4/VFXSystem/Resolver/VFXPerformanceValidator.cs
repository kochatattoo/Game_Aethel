using UnityEngine;
using VFXSystem.Parameters.Settings;

namespace VFXSystem.Resolver
{
    public class VFXPerformanceValidator<T> : IVFXValidator<T>
    {
        private readonly float _minFPS;

        private float _currentFps;
        private float _deltaTime;
        private const float SmoothingFactor = 0.1f;

        public VFXPerformanceValidator(VFXRestrictionSettings settings)
        {
            _currentFps = settings.CurrentFPS;
            _minFPS = settings.MinFPS;
        }

        public bool CanSpawn(VFXSpawnContext<T> context)
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * SmoothingFactor;
            _currentFps = 1.0f / _deltaTime;

            return _currentFps >= _minFPS;
        }

        public void OnEffectSpawned(T definition) { }
        public void OnEffectDespawned(T definition) { }
    }
}
