using UnityEngine;

namespace VFXSystem.Resolver
{
    public class VFXFrustumValidator<T> : IVFXValidator<T>
    {
        private const float Margin = 0.1f;
        private  Camera _camera;

        public VFXFrustumValidator()  
        { 
            //_camera = camera;  вернут ьпозже через DI
            _camera = Camera.main;
        }

        public bool CanSpawn(VFXSpawnContext<T> context)
        {
            if (_camera == null) 
                _camera = Camera.main;

            Vector3 viewportPoint = _camera.WorldToViewportPoint(context.Position);

            return viewportPoint.z > 0 &&
                   viewportPoint.x >= -Margin && viewportPoint.x <= 1 + Margin &&
                   viewportPoint.y >= -Margin && viewportPoint.y <= 1 + Margin;
        }

        public void OnEffectSpawned(T definition) { }
        public void OnEffectDespawned(T definition) { }
    }
}
