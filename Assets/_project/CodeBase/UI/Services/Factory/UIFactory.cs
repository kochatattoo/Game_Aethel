using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Ads;
using CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.StaticData.Windows;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
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
        private readonly IPoolService _poolService;
        private Transform _uiRoot;

        public UIFactory(IAsset assets,
                    IStaticDataService staticData,
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
                if (id == WindowId.Unknow)
                    continue;

                WindowConfig config = _staticData.ForWindow(id);
                WindowBase prefab = config.prefab;

                Type windowType = prefab.GetType();

                MethodInfo registerMethod = typeof(IPoolService)
                    .GetMethod(nameof(IPoolService.AddPoolToParent), BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(windowType);

                registerMethod.Invoke(_poolService, new object[] { prefab, _uiRoot, 1 });
            }
        }

        public async UniTask WarmUpAsync()
        {
            var tasks = new List<UniTask>();

            foreach (WindowId id in Enum.GetValues(typeof(WindowId)))
            {
                if (id == WindowId.Unknow)
                    continue;

                WindowConfig config = _staticData.ForWindow(id);
                WindowBase prefab = config.prefab;
                Type windowType = prefab.GetType();

                // Находим через Reflection ваш метод AddPoolToParentAsync<T>
                MethodInfo mi = typeof(IPoolService)
                    .GetMethod(nameof(IPoolService.AddPoolToParentAsync),
                               BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(windowType);

                // Вызываем его и получаем UniTask
                // Вариант A: явно кастуем
                var taskObj = mi.Invoke(_poolService, new object[] { prefab, _uiRoot, 1 });
                var ut = (UniTask)taskObj;

                // Собираем в список, чтобы запустить всё параллельно и потом ждать всех разом
                tasks.Add(ut);
            }

            await UniTask.WhenAll(tasks);
        }

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
