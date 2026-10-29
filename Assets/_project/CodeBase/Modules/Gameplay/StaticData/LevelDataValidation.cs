using System;
using System.Collections.Generic;

namespace CodeBase.StaticData
{
    public static class LevelDataValidation
    {
        /// <summary>Preserves stable save IDs, coalesces exact duplicates and rejects ambiguous identities.</summary>
        public static IReadOnlyList<EnemySpawnerData> UniqueSpawners(IEnumerable<EnemySpawnerData> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var byId = new Dictionary<string, EnemySpawnerData>(StringComparer.Ordinal);
            var result = new List<EnemySpawnerData>();
            foreach (var spawner in source)
            {
                if (spawner == null || string.IsNullOrWhiteSpace(spawner.Id))
                    throw new ArgumentException("Enemy spawner needs a stable nonempty ID");
                if (byId.TryGetValue(spawner.Id, out var existing))
                {
                    if (existing.MonsterTypeID != spawner.MonsterTypeID || !existing.position.Equals(spawner.position))
                        throw new ArgumentException("Conflicting enemy spawner ID: " + spawner.Id);
                    continue;
                }
                byId.Add(spawner.Id, spawner);
                result.Add(spawner);
            }
            return result;
        }
    }
}
