using System.Collections.Generic;
using VFXSystem.Parameters.Settings;

namespace VFXSystem.Resolver
{
    public class VFXCountValidator<T> : IVFXValidator<T>
    {
        private readonly VFXRestrictionSettings _config;
        private readonly Dictionary<T, int> _activeCounts = new Dictionary<T, int>();

        public VFXCountValidator(VFXRestrictionSettings config)
        {
            _config = config;
        }

        public bool CanSpawn(VFXSpawnContext<T> context)
        {
            if (_config.MaxCount <= 0)
                return true;

            if (_activeCounts.TryGetValue(context.Definition, out int count))
            {
                return count < _config.MaxCount;
            }

            return true;
        }

        public void OnEffectSpawned(T effectId)
        {
            if (!_activeCounts.TryAdd(effectId, 1))
            {
                _activeCounts[effectId]++;
            }
        }

        public void OnEffectDespawned(T effectId)
        {
            if (_activeCounts.TryGetValue(effectId, out int count))
            {
                _activeCounts[effectId] = count > 0 ? count - 1 : 0;
            }
        }
    }
}
