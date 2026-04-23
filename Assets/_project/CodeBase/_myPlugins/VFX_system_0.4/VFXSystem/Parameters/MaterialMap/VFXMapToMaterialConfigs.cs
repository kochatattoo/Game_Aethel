using Shared.Utils.Constants;
using System.Collections.Generic;
using UnityEngine;

namespace VFXSystem.Parameters.MaterialMap
{
    [CreateAssetMenu(fileName = nameof(VFXMapToMaterialConfigs), menuName = ScriptableObjectNames.VFXName + "Mapping/" + nameof(VFXMapToMaterialConfigs))]
    public class VFXMapToMaterialConfigs : ScriptableObject
    {
        [SerializeField]
        private List<VFXToMaterial> _vfxList = new List<VFXToMaterial>();

        public IReadOnlyList<VFXToMaterial> VFXToMaterials => _vfxList;
    }
}
