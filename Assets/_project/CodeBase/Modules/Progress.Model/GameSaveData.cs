using System;
using System.Collections.Generic;
using Code.Hero.Data;
using CodeBase.Data;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    [Serializable] public sealed class GameSaveData
    {
        public ProfileProgress Profile;
        public LevelProgress Level;
        public RunProgress Run;
    }
    [Serializable] public sealed class ProfileProgress
    {
        public int CharacterSchemaVersion;
        public CharacterProfileData Character;
        public CodeBase.Data.State Health;
        public Stats Stats;
        public int Coins;
        public List<PurchaseEntry> Purchases = new();
    }
    [Serializable] public sealed class PurchaseEntry { public string Id; public int Count; }
    [Serializable] public sealed class LevelProgress
    {
        public string LevelId;
        public bool HasPosition;
        public Vector3Data Position;
        public List<string> ClearedSpawners = new();
        public List<LootObject> GroundLoot = new();
    }
    public enum RunStatus { None, Active, Interrupted, Completed }
    [Serializable] public sealed class RunProgress
    {
        public string RunId;
        public RunStatus Status;
        public string HubLevelId;
        public bool RewardSettled;
    }

    public static class GameSaveAdapter
    {
        public static GameSaveData Capture(PlayerProgress progress, string safeLevel)
        {
            var profile = new ProfileProgress
            {
                CharacterSchemaVersion = progress.CharacterSchemaVersion, Character = progress.Character,
                Health = progress.HeroState, Stats = progress.HeroStats, Coins = progress.WorldData.LootData.Collected
            };
            foreach (var pair in progress.PurchaseData.BoughtIAPs.Dict)
                profile.Purchases.Add(new PurchaseEntry { Id = pair.Key, Count = pair.Value.Count });
            var level = new LevelProgress
            {
                LevelId = progress.WorldData.PositionOnLevel.Level, Position = progress.WorldData.PositionOnLevel.Position,
                HasPosition = progress.WorldData.PositionOnLevel.Position != null,
                ClearedSpawners = new List<string>(progress.KillData.ClaeredSpawners)
            };
            foreach (var pair in progress.WorldData.LootData.LootsOnGround.Dict) level.GroundLoot.Add(pair.Value);
            return new GameSaveData { Profile = profile, Level = level, Run = new RunProgress { HubLevelId = safeLevel } };
        }

        public static PlayerProgress Restore(GameSaveData data)
        {
            if (data?.Profile?.Health == null || data.Profile.Stats == null || data.Profile.Purchases == null ||
                data.Level?.ClearedSpawners == null || data.Level.GroundLoot == null || data.Run == null ||
                data.Profile.Coins<0 || !Enum.IsDefined(typeof(RunStatus), data.Run.Status)) throw new ArgumentException("Неполное сохранение");
            if (data.Run.Status != RunStatus.None && !Guid.TryParseExact(data.Run.RunId, "N", out _)) throw new ArgumentException("Неверный runId");
            var interrupted = data.Run.Status == RunStatus.Active || data.Run.Status == RunStatus.Interrupted;
            var progress = new PlayerProgress(interrupted ? data.Run.HubLevelId : data.Level.LevelId)
            {
                CharacterSchemaVersion = data.Profile.CharacterSchemaVersion, Character = data.Profile.Character,
                HeroState = data.Profile.Health, HeroStats = data.Profile.Stats
            };
            progress.WorldData.LootData.Collected = data.Profile.Coins;
            foreach (var item in data.Profile.Purchases)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id) || item.Count < 0 ||
                    !progress.PurchaseData.BoughtIAPs.Dict.TryAdd(item.Id, new BoughtIAP(item.Id, item.Count))) throw new ArgumentException("Неверные покупки");
            }
            if (!interrupted)
            {
                progress.WorldData.PositionOnLevel.Position = data.Level.HasPosition ? data.Level.Position : null;
                progress.KillData.ClaeredSpawners = new List<string>(data.Level.ClearedSpawners);
                foreach (var loot in data.Level.GroundLoot)
                    if (loot == null || string.IsNullOrEmpty(loot.id) || loot.positionData == null || loot.loot == null ||
                        !progress.WorldData.LootData.LootsOnGround.Dict.TryAdd(loot.id, loot)) throw new ArgumentException("Неверная добыча");
            }
            return progress;
        }
    }
}
