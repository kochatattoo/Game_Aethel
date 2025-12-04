using CodeBase.Hero;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using System.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IInputService _inputService;
        private readonly IAsset _assets;

        private GameObject HeroGameObject { get; set; }

        public GameFactory(IInputService inputService, IAsset asset)
        {
            _inputService = inputService;
            _assets = asset;
        }

        public async Task<GameObject> CreateHero(Vector3 at)
        {
            HeroGameObject = await InstantiateRegisteredAsync(AssetAddress.HeroParh, at);

            HeroGameObject.GetComponent<HeroMove>()
               .Construct(_inputService);

            return HeroGameObject;
        }

        private async Task<GameObject> InstantiateRegisteredAsync(string path, Vector3 at)
        {
            GameObject gameObject = await _assets.Instantiate(path, at);
            return gameObject;
        }
    }
}
