using Shared.Utils.Constants;
using System.Collections.Generic;
using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.Parameters.Mapping;

namespace VFXSystem.Parameters.Definition
{
    [CreateAssetMenu(fileName = nameof(VFXDefinitionsMapConfigs), menuName = ScriptableObjectNames.VFXName + "Mapping/" + nameof(VFXDefinitionsMapConfigs))]
    public class VFXDefinitionsMapConfigs : ScriptableObject, IMapConfigs<MaterialType, HitVFXDefinition>
    {
        [SerializeField]
        private List<HitVFXDefinition> _definitions = new();

        public IReadOnlyList<IValue<MaterialType, HitVFXDefinition>> Definitions => _definitions;
    }
}
