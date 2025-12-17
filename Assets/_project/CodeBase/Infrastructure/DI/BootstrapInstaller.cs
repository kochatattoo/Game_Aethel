using Assets._project.CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Ads;
using CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure.Services.Levels;
using CodeBase.Infrastructure.Services.LogData;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Infrastructure.State;
using CodeBase.Logic;
using CodeBase.UI.Services.Factory;
using CodeBase.UI.Services.Windows;
using System;
using UnityEngine;
using Zenject;

namespace CodeBase.DI
{
    public class BootstrapInstaller : MonoInstaller
    {
        public GameObject CoroutineRunner;
        public GameObject LoadingCurtain;

        public override void InstallBindings()
        {
            BindDIFactory();
            BindGameStateMachine();

            BindCoroutineRunner();
            BindSceneLoader();
            BindLoadingCurtain();

            BindServices();

            BindStates();
        }

        private void BindDIFactory()
        {
            BindServiceFactory();
            BindStateFactory();
        }

        private void BindServiceFactory() => 
            Container.Bind<IServiceFactory>().To<ServiceFactory>()
                     .AsSingle()
                     .NonLazy();

        private void BindStateFactory() =>
            Container.Bind<IStateFactory>()
                     .To<StateFactory>()
                     .AsSingle()
                     .NonLazy();

        private void BindGameStateMachine() =>
            Container.BindInterfacesTo<GameStateMachine>()
                .AsSingle()
                .NonLazy();

        private void BindCoroutineRunner()
        {
            CoroutineRunner coroutineRunner = Container
                .InstantiatePrefabForComponent<CoroutineRunner>(CoroutineRunner);

            Container
                .Bind<ICoroutineRunner>()
                .FromInstance(coroutineRunner)
                .AsSingle()
                .NonLazy();
        }

        private void BindSceneLoader() => 
            Container.Bind<SceneLoader>()
                     .AsSingle()
                     .NonLazy();

        private void BindLoadingCurtain()
        {
            LoadingCurtain loadingCurtain =
                Container.InstantiatePrefabForComponent<LoadingCurtain>(LoadingCurtain);

            Container
                .Bind<LoadingCurtain>()
                .FromInstance(loadingCurtain)
                .AsSingle()
                .NonLazy();
        }

        private void BindServices()
        {
            BindLogDataService();
            BindAdsService();
            BindAssetProvider();
            BindStaticData();
            BindInputService();
            BindRandom();
            BindPersistentProgress();
            BindIAPService();
            BindWindowService();
            BindLevelTransfer();
            BindReload();
            BindSaveLoad();
            BindGameFactory();
            BindUIFactory();
        }

        private void BindLogDataService() =>
            Container.BindInterfacesTo<LogDataService>()
                .AsSingle()
                .NonLazy();

        private void BindAdsService() =>
            Container.BindInterfacesTo<AdsService>()
                .AsSingle()
                .NonLazy();

        private void BindIAPService() =>
            Container.BindInterfacesTo<IAPService>()
            .AsSingle()
            .NonLazy();

        private void BindAssetProvider() => 
            Container.Bind<IAsset>()
                     .To<AssetProvider>()
                     .AsSingle()
                     .NonLazy();

        private void BindStaticData() => 
            Container.Bind<IStaticDataService>()
                     .To<StaticDataService>()
                     .AsSingle()
                     .NonLazy();

        private void BindInputService() =>
            Container.BindInterfacesTo<NewInputService>()
                     .AsSingle()
                     .NonLazy();

        private void BindRandom() => 
            Container.Bind<IRandomService>()
                                 .To<UnityRandomService>()
                                 .AsSingle()
                                 .NonLazy();

        private void BindPersistentProgress() => 
            Container.Bind<IPersistentProgressService>()            
                                .To<PersistentProgressService>()
                                .AsSingle()
                                .NonLazy();

        private void BindWindowService() =>
            Container.BindInterfacesTo<WindowService>()
               .AsSingle()
               .NonLazy();

        private void BindLevelTransfer() =>
            Container.Bind<ILevelTransferService>()
                     .To<LevelTransferService>()
                     .AsSingle()
                     .NonLazy();

        private void BindReload() => 
            Container.Bind<IReloadService>()
                     .To<ReloadService>()
                     .AsSingle()
                     .NonLazy();

        private void BindSaveLoad() =>
         Container.BindInterfacesTo<SaveLoadService>()
                  .AsSingle()
                  .NonLazy();

        private void BindGameFactory() => 
            Container.Bind<IGameFactory>()
                     .To<GameFactory>()
                     .AsSingle()
                     .NonLazy();

        private void BindUIFactory() =>
            Container.Bind<IUIFactory>()
                     .To<UIFactory>()
                     .AsSingle()
                     .NonLazy();

        private void BindStates()
        {
            Container.Bind<BootstrapState>().AsSingle().NonLazy();
            Container.Bind<LoadProgressState>().AsSingle().NonLazy();
            Container.Bind<LoadLevelState>().AsSingle().NonLazy();
            Container.Bind<GameLoopState>().AsSingle().NonLazy();
        }
    }
}