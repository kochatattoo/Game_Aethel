using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.AudioSystem.Parameters
{
    public class WwiseSwitchMaping<T>
    {
        private readonly BaseWwiseToMaterialConfig<T> _wwiseMaterialConfig;
        private readonly Dictionary<Material, T> _map = new();

        public IReadOnlyDictionary<Material, T> Map => _map;

        public WwiseSwitchMaping(BaseWwiseToMaterialConfig<T> wwiseMaterialConfig)
        {
            _wwiseMaterialConfig = wwiseMaterialConfig;

            foreach (var materialConfig in _wwiseMaterialConfig.Material)
            {
                foreach (var material in materialConfig.Materials)
                {
                    if (!_map.TryAdd(material, materialConfig.SwitchValue))
                    {
                        Debug.LogWarning($"Дубликат материала {material.name} в конфиге!");
                        continue;
                    }
                }
            }
        }
    }
}
