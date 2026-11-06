using System.Threading.Tasks;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public sealed class WorldAssetFactory
    {
        private readonly IAsset _assets;
        private readonly IProgressRegistry _registry;
        public WorldAssetFactory(IAsset assets, IProgressRegistry registry) { _assets = assets; _registry = registry; }

        public async Task<GameObject> CreateAsync(string address, Vector3? position = null)
        {
            var instance = position.HasValue ? await _assets.Instantiate(address, position.Value) : await _assets.Instantiate(address);
            foreach (var reader in instance.GetComponentsInChildren<ISavedProgressReader>()) _registry.Register(reader);
            return instance;
        }
    }
}
