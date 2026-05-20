using System.Collections.Generic;
using UnityEngine;
using VFXSystem.Parameters.Settings;

namespace VFXSystem.Resolver
{
    public class VFXCooldownValidator<T> : IVFXValidator<T>
    {
        private readonly VFXRestrictionSettings _config;
        private readonly Dictionary<T, float> _lastSpawnTimes = new();

        public VFXCooldownValidator(VFXRestrictionSettings config)
        {
            _config = config;
        }

        public bool CanSpawn(VFXSpawnContext<T> context)
        {
            float cooldown = _config.MinSpawnInterval;

            if (cooldown <= 0) 
                return true;

            if (_lastSpawnTimes.TryGetValue(context.Definition, out float lastTime))
            {
                if (Time.time < lastTime + cooldown) 
                    return false;
            }

            return true;
        }

        public void OnEffectSpawned(T definition) => _lastSpawnTimes[definition] = Time.time;
        public void OnEffectDespawned(T definition) { }
    }
}
