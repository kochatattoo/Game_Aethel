using UnityEngine;
using VFXSystem.Parameters.Settings;

namespace VFXSystem.Resolver
{
    public class VFXDistanceValidator<T> : IVFXValidator<T> 
    {
        private readonly Camera _mainCamera;
        private readonly VFXRestrictionSettings _config;
        private readonly float _sqrMaxDistance;

        public VFXDistanceValidator(VFXRestrictionSettings config, Camera camera)  
        { 
            _config = config;
            _mainCamera = camera;

            _sqrMaxDistance = _config.MaxDistance * _config.MaxDistance;
        }

        public bool CanSpawn(VFXSpawnContext<T> context)
        {
            if (_mainCamera == null) 
                return false;

            Vector3 offset = context.Position - _mainCamera.transform.position;
            float sqrDist = offset.sqrMagnitude;

            return sqrDist <= _sqrMaxDistance;
        }

        public void OnEffectSpawned(T definition) { /* Не требуется */ }
        public void OnEffectDespawned(T definition) { /* Не требуется */ }
    }
}
