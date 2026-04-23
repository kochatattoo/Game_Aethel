using System;
using UnityEngine;
using VFXSystem.Parameters.Definition;
using VFXSystem.Parameters.Mapping;

namespace VFXSystem.Quality
{
    [Serializable]
    public class VFXQualityDefinition: IValue<VFXQuality, VFXDefinitionsMapConfigs>
    {
        [field: SerializeField] 
        public VFXQuality Key { get; private set; }

        [field: SerializeField]
        public VFXDefinitionsMapConfigs Value { get; private set; }
    }
}
