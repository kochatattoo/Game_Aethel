using System.Collections.Generic;
using UnityEngine;

namespace VFXSystem.Parameters.Mapping
{
    public class Mapping<Tkey, Tvalue> : IMap<Tkey, Tvalue>
    {
        private readonly Dictionary<Tkey, Tvalue> _effects = new();
        public IReadOnlyDictionary<Tkey, Tvalue> Effects => _effects;

        public Mapping(IMapConfigs<Tkey, Tvalue> configs)
        {
            foreach (var config in configs.Definitions)
            {
                var materialType = config.Key;

                if (_effects.ContainsKey(materialType))
                {
                    Debug.Log($"This {config.Value} for {materialType} contains in map");
                    continue;
                }
                else
                {
                    _effects[materialType] = config.Value;
                }
            }
        }
    }
}
