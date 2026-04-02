using UnityEngine;
using VFXSystem.Parameters.Definition;

namespace VFXSystem.Factory
{
    /// <summary>
    /// Класс - который берет VFX из пула и передает ему параметры (используется в фасаде)
    /// </summary>
    public class VFXFactory : IVFXFactory
    {
        private readonly VFXEntity.Pool _pool;

        public VFXFactory(VFXEntity.Pool pool) => _pool = pool;

        public VFXEntity CreateVFXEntity(HitVFXDefinition definition) => _pool.Spawn(definition);
    }
}
