using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Ads;
using CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.StaticData.Windows;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows;
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
        private readonly ISaveLoadService _saveLoadService;
        private readonly IReloadService _reloadService;
        private readonly IInputService _inputService;
        private readonly IAdsService _adsService;
        private readonly IIAPService _iapService;
        private Transform _uiRoot;

        public UIFactory(IAsset assets,
                    IStaticDataService staticData,
                    IPersistentProgressService progressService,
                    IInputService inputService,
                    IReloadService reloadService,
                    ISaveLoadService saveLoadService,
                    IAdsService adsService,
                    IIAPService iapService)
        {
            _assets = assets;
            _staticData = staticData;
            _progressService = progressService;
            _reloadService = reloadService;
            _saveLoadService = saveLoadService;
            _inputService = inputService;
            _adsService = adsService;
            _iapService = iapService;
        }

        public void CreateOption()
        {
            OptionWindow optWindow = CreateWindow<OptionWindow>(WindowId.Option);
            optWindow.Construct(_saveLoadService, _progressService, _reloadService, _inputService);
        }

        public void CreateShop()
        {
            ShopWindow window = CreateWindow<ShopWindow>(WindowId.Shop);
            window.Construct(_adsService, _progressService, _iapService, _assets);
        }

        public async Task CreateUIRoot()
        {
            GameObject pref = await _assets.Load<GameObject>(AssetAddress.UIRoot);
            _uiRoot = Object.Instantiate(pref).transform;
        }

        private T CreateWindow<T>(WindowId ind) where T : WindowBase
        {
            WindowConfig config = _staticData.ForWindow(ind);
            T window = Object.Instantiate(config.prefab, _uiRoot) as T;
            return window;
        }
    }
}
