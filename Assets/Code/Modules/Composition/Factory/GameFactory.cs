using System.Threading.Tasks;
using CodeBase.Enemies;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.Levels;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Logic;
using CodeBase.StaticData;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    /// <summary>Фасад добычи, триггеров и переходов между уровнями.</summary>
    public sealed class GameFactory : IGameFactory
    {
        private readonly IAsset _assets;
        private readonly WorldAssetFactory _instances;
        private readonly ILootFactory _loot;
        private readonly IProgressRegistry _registry;
        private readonly ISaveLoadService _save;
        private readonly ILevelTransferService _transfers;

        public GameFactory(IAsset assets, WorldAssetFactory instances, ILootFactory loot,
            IProgressRegistry registry, ISaveLoadService save, ILevelTransferService transfers)
        {
            _assets = assets;
            _instances = instances;
            _loot = loot;
            _registry = registry;
            _save = save;
            _transfers = transfers;
        }

        public UniTask WarmUpAsync() => _loot.WarmUpAsync();

        public Task<LootPiece> CreateLoot() => _loot.CreateAsync();
        public async Task<LootPiece> CreateLoot(string id) { var loot = await CreateLoot(); loot.SetId(id); return loot; }
        public UniTask<LootPiece> CreateLootFromPool() => _loot.SpawnAsync();
        public async UniTask<LootPiece> CreateLootFromPool(string id) { var loot = await CreateLootFromPool(); loot.SetId(id); return loot; }

        public async Task CreateSaveTrigger(Vector3 position, string id)
        {
            var trigger = (await _instances.CreateAsync(AssetAddress.SaveTrigger, position)).GetComponent<SaveTrigger>();
            trigger.Construct(_save);
            trigger.Id = id;
        }

        public async Task CreateTransferToPoint(LevelTransferData data)
        {
            var instance = await _instances.CreateAsync(AssetAddress.TransferToPoint, data.TransferToPosition);
            instance.GetComponent<LevelTransferTrigger>().Construct(_transfers, data.LevelTo);
        }

        public void CleanUp()
        {
            _loot.DespawnAll();
            _registry.Clear();
            _assets.CleanUp();
        }
    }
}
