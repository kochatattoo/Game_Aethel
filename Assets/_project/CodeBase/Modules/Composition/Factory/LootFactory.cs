using System.Threading.Tasks;
using CodeBase.Enemies;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.Factory
{
    public sealed class LootFactory : ILootFactory
    {
        private readonly IAsset _assets;
        private readonly WorldAssetFactory _instances;
        private readonly IPoolService _pools;
        private readonly IPersistentProgressService _progress;
        private readonly IProgressRegistry _registry;
        public LootFactory(IAsset assets, WorldAssetFactory instances, IPoolService pools,
            IPersistentProgressService progress, IProgressRegistry registry)
        {
            _assets = assets;
            _instances = instances;
            _pools = pools;
            _progress = progress;
            _registry = registry;
        }

        public async UniTask WarmUpAsync()
        {
            var prefab = await _assets.Load<UnityEngine.GameObject>(AssetAddress.Loot);
            await _pools.AddPoolToContainerAsync(prefab.GetComponent<LootPiece>(), 5);
        }

        public async Task<LootPiece> CreateAsync()
        {
            var loot = (await _instances.CreateAsync(AssetAddress.Loot)).GetComponent<LootPiece>();
            Initialize(loot);
            return loot;
        }

        public async UniTask<LootPiece> SpawnAsync()
        {
            var loot = await _pools.GetPool<LootPiece>().SpawnExpandableAsync(initializer: Initialize);
            _registry.Register(loot);
            return loot;
        }

        private void Initialize(LootPiece loot) => loot.Construct(_progress.Progress.WorldData, _registry);
        public void DespawnAll() => _pools.GetPool<LootPiece>()?.DespawnAllActive();
    }
}
