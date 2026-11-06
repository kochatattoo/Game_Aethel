using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.AudioSystem.Parameters
{
    public abstract class BaseWwiseToMaterialConfig<T> : ScriptableObject
    {
        [SerializeField]
        protected List<WwiseToMaterial<T>> _materials;

        public IReadOnlyList<WwiseToMaterial<T>> Material => _materials;
    }
}
