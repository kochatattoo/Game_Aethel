using Infrastructure.AudioSystem.Parameters;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.MaterialConfigs
{
    public abstract class BaseSurfaceResolverConfig<T> : ScriptableObject
    {
        [Header("Raycast Settings")]
        [field: SerializeField]
        public CastSettings CastSettings { get; private set; }

        [field: SerializeField]
        public BaseWwiseToMaterialConfig<T> WwiseToMaterial { get; private set; }

        public abstract T DefaultSwitch { get; }
        public abstract IEnumerable<(string Tag, T Key)> GetTagMappings();
    }
}
