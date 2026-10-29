using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Ads;
using CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;

using CodeBase.StaticData.Windows;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.UI.Services.Factory
{
    public class UIFactory : IUIFactory
    {
        private readonly IAsset _assets;
        private readonly IWindowCatalog _staticData;
        private readonly IPersistentProgressService _progressService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly IReloadService _reloadService;
        private readonly IInputService _inputService;
        private readonly IAdsService _adsService;
        private readonly IIAPService _iapService;
        private readonly IPoolService _poolService;
        private Transform _uiRoot;
        public Transform Root => _uiRoot;

        public UIFactory(IAsset assets,
                    IWindowCatalog staticData,
                    IPersistentProgressService progressService,
                    IInputService inputService,
                    IReloadService reloadService,
                    ISaveLoadService saveLoadService,
                    IAdsService adsService,
                    IIAPService iapService,
                    IPoolService poolService)
        {
            _assets = assets;
            _staticData = staticData;
            _progressService = progressService;
            _reloadService = reloadService;
            _saveLoadService = saveLoadService;
            _inputService = inputService;
            _adsService = adsService;
            _iapService = iapService;
            _poolService = poolService;
        }

        public async Task<GameObject> CreateHud(CodeBase.Logic.IHealth health)
        {
            var hud = await _assets.Instantiate(AssetAddress.HUDPath);
            hud.GetComponentInChildren<CodeBase.UI.Elements.LootCounter>().Construct(_progressService.Progress.WorldData);
            hud.GetComponentInChildren<CodeBase.UI.Elements.ActorUI>().Construct(health);
            var windows = new CodeBase.UI.Services.Windows.WindowService(this);
            foreach (var button in hud.GetComponentsInChildren<CodeBase.UI.Elements.OpenWindowButton>()) button.Construct(windows);
            return hud;
        }

        public void CreateOption()
        {
            OptionWindow optWindow = CreateWindow<OptionWindow>(w => 
            w.Construct(_progressService, 
                        _saveLoadService,
                        _reloadService, 
                        _inputService));
        }

        public async UniTask CreateOptionAsync()
        {
           OptionWindow optWindow =await CreateWindowAsync<OptionWindow>(w =>
           w.Construct(_progressService,
                       _saveLoadService,
                       _reloadService,
                       _inputService));
        }

        public void CreateShop()
        {
            ShopWindow window = CreateWindow<ShopWindow>(w =>
            w.Construct(_progressService,  
                        _adsService,
                        _iapService, 
                        _assets));
        }

        public async UniTask CreateShopAsync()
        {
            ShopWindow window = await CreateWindowAsync<ShopWindow>(w =>
            w.Construct(_progressService,
                        _adsService,
                        _iapService,
                        _assets));
        }

        public async UniTask CreateUIRootAsync()
        {
            GameObject pref = await _assets.Load<GameObject>(AssetAddress.UIRoot);
            _uiRoot = Object.Instantiate(pref).transform;
            
            GameObject.DontDestroyOnLoad(_uiRoot);
        }

        public void WarmUp()
        {
            foreach (WindowId id in Enum.GetValues(typeof(WindowId)))
            {
                if (id == WindowId.Unknow) continue;
                var prefab = _staticData.ForWindow(id).prefab;
                if (prefab is OptionWindow option) _poolService.AddPoolToParent(option, _uiRoot, 1);
                else if (prefab is ShopWindow shop) _poolService.AddPoolToParent(shop, _uiRoot, 1);
                else throw new InvalidOperationException("Не зарегистрирован тип окна: " + prefab.GetType().Name);
            }
        }

        public UniTask WarmUpAsync() { WarmUp(); return UniTask.CompletedTask; }

        private T CreateWindow<T>(Action<T> initializer) where T : WindowBase
        {
            var pool = _poolService.GetPool<T>();
            T window = pool.Spawn(_uiRoot, initializer);
            return window;
        }

        private T CreateWindow<T>(WindowId ind) where T : WindowBase
        {
            WindowConfig config = _staticData.ForWindow(ind);
            T window = Object.Instantiate(config.prefab, _uiRoot) as T;
            return window;
        }

        private async UniTask<T> CreateWindowAsync<T>(Action<T> initializer) where T : WindowBase
        {
            var pool = _poolService.GetPool<T>();
            T window = await pool.SpawnAsync(_uiRoot, initializer);
            return window;
        }
    }
}
