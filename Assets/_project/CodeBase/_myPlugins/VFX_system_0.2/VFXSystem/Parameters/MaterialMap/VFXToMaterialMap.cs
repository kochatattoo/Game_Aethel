using System.Collections.Generic;
using UnityEngine;
using VFXSystem.BaseTypes;

namespace VFXSystem.Parameters.MaterialMap
{
    public class VFXToMaterialMap
    {
        private readonly VFXMapToMaterialConfigs _configs;
        private readonly Dictionary<Material, MaterialType> _map = new();

        public IReadOnlyDictionary<Material, MaterialType> Map => _map;

        public VFXToMaterialMap (VFXMapToMaterialConfigs configs)
        {
            _configs = configs;

            foreach (var materialConfig in _configs.VFXToMaterials)
            {
                foreach (var material in materialConfig.Materials)
                {
                    if (!_map.TryAdd(material, materialConfig.Material))
                    {
                        Debug.LogWarning($"Дубликат материала {material.name} в конфиге!");
                        continue;
                    }
                }
            }
        }
    }
}
