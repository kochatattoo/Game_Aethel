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

            int totalMaterials = 0;
            foreach (var config in _configs.VFXToMaterials)
            {
                totalMaterials += config.Materials.Count;
            }

            _map = new Dictionary<Material, MaterialType>(totalMaterials);

            foreach (var materialConfig in _configs.VFXToMaterials)
            {
                var type = materialConfig.Material;
                var materials = materialConfig.Materials;

                for (int i = 0; i < materials.Count; i++)
                {
                    var mat = materials[i];
                    if (mat == null) 
                        continue;

                    if (!_map.TryAdd(mat, type))
                    {
                        Debug.LogWarning($"[VFX] Дубликат материала {mat.name} для типа {type}!");
                    }
                }
            }
        }

        /// <summary>
        /// Метод для поиска без аллокаций в рантайме
        /// </summary>
        public bool TryGetMaterialType(Material material, out MaterialType type)
        {
            if (material == null)
            {
                type = MaterialType.None; 
                return false;
            }

            return _map.TryGetValue(material, out type);
        }
    }
}
