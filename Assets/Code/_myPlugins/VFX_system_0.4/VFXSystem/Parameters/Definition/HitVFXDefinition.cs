using Shared.Utils.Constants;
using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.VFXTypes;
using VFXSystem.Parameters.Mapping;

namespace VFXSystem.Parameters.Definition
{
    [CreateAssetMenu(fileName = nameof(HitVFXDefinition), menuName = ScriptableObjectNames.VFXName + nameof(HitVFXDefinition))]
    public class HitVFXDefinition : ScriptableObject, IValue<MaterialType, HitVFXDefinition>
    {
        public HitVFXDefinition Value =>this;

        [field: SerializeField]
        public MaterialType Key {get; private set;}

        [field: SerializeField]
        public ParticleSubVFX ParticlePrefab {get; private set;}

        [field: SerializeField]
        public DecalSubVFX DecalPrefab {get; private set;} //На поверхности - сила импакта увеличивает/уменьшает скайл

        [field: SerializeField]
        public AdditionalSubVFX AdditionalEffectPrefab {get; private set;} // От силы импакта

    }
}
