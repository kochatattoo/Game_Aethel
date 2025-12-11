using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.StaticData.Windows;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.UI.Services.Factory
{
    public class UIFactory : IUIFactory
    {
        private readonly IAsset _assets;
        private readonly IStaticDataService _staticData;
        private readonly IPersistentProgressService _progressService;

        private Transform _uiRoot;

        public UIFactory(IAsset assets,
                    IStaticDataService staticData,
                    IPersistentProgressService progressService
                   )
        {
            _assets = assets;
            _staticData = staticData;
            _progressService = progressService;
        }

        public void CreateShop()
        {
            //WindowConfig config = _staticData.ForWindow(WindowId.Shop);
            //ShopWindow window = Object.Instantiate(config.prefab, _uiRoot) as ShopWindow;
            //window.Construct(_adsService, _progressService, _iapService, _assets);
        }

        public async Task CreateUIRoot()
        {
            GameObject pref = await _assets.Load<GameObject>(AssetAddress.UIRoot);
            _uiRoot = Object.Instantiate(pref).transform;
        }
    }
}
