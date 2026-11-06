using JetBrains.Annotations;
using Shared.Utils.Constants;
using System.Collections.Generic;
using UnityEngine;
using VFXSystem.Parameters.Definition;
using VFXSystem.Parameters.Mapping;
using VFXSystem.Quality;

namespace VFXSystem.Parameters.Settings
{
    [CreateAssetMenu(fileName = nameof(VFXQualitySettings), menuName = ScriptableObjectNames.VFXName + "Mapping/" + nameof(VFXQualitySettings))]
    public class VFXQualitySettings : ScriptableObject, IMapConfigs<VFXQuality, VFXDefinitionsMapConfigs>
    {
        [Header("Fallbacks"), CanBeNull]
        public HitVFXDefinition DefaultLowQualityEffect;

        [Header("Configs"), SerializeField]
        private List<VFXQualityDefinition> _vfxList = new List<VFXQualityDefinition>();

        public IReadOnlyList<IValue<VFXQuality, VFXDefinitionsMapConfigs>> Definitions => _vfxList;
    }
}
