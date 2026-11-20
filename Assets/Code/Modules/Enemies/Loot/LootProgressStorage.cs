using CodeBase.Data;
using UnityEngine;

namespace CodeBase.Enemies
{
    internal static class LootProgressStorage
    {
        public static void Save(PlayerProgress progress, string id, Vector3 position, Loot loot)
        {
            var loots = progress.WorldData.LootData.LootsOnGround.Dict;
            if (!loots.ContainsKey(id)) loots.Add(id, new LootObject(id, position, loot));
        }

        public static bool TryTake(PlayerProgress progress, string id, out LootObject loot)
        {
            var loots = progress.WorldData.LootData.LootsOnGround.Dict;
            if (!loots.TryGetValue(id, out loot)) return false;
            loots.Remove(id);
            return true;
        }

        public static void Collect(WorldData worldData, string id, Loot loot)
        {
            worldData.LootData.LootsOnGround.Dict.Remove(id);
            worldData.LootData.Collect(loot);
        }
    }
}
