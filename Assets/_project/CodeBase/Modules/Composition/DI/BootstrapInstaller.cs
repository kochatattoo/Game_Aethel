using CodeBase.Composition;
using Assets._project.CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using Assets._project.CodeBase.Infrastructure.Services.Input;
using CodeBase.Infrastructure;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Ads;
using CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure.Services.Levels;
using CodeBase.Infrastructure.Services.LogData;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Infrastructure.State;
using CodeBase.Logic;
using CodeBase.UI.Services.Factory;
using CodeBase.UI.Services.Windows;
using UnityEngine;
using Zenject;
using CodeBase.CharacterIntegration;

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
            BindGame();
        }

        private void BindDIFactory()
        {
            BindStateFactory();
        }

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
                .BindInterfacesAndSelfTo<LoadingCurtain>()
                .FromInstance(loadingCurtain)
                .AsSingle()
                .NonLazy();
        }

        private void BindServices()
        {
            Container.BindInterfacesAndSelfTo<CharacterAssets>().AsSingle();
            Container.BindInterfacesAndSelfTo<CharacterMenuPresentation>().AsSingle();
            Container.BindInterfacesAndSelfTo<CharacterSession>().AsSingle();
            Container.Bind<IApplicationHost>().FromMethod(_ => ApplicationHost.Current).AsSingle();
            Container.BindInterfacesTo<MenuShortcutHandler>().AsSingle();
            Container.Bind<IApplicationWarmup>().To<ApplicationWarmup>().AsSingle();
            Container.Bind<IWorldBuilder>().To<GameWorldBuilder>().AsSingle();
            Container.Bind<IWindowCatalog>().To<WindowCatalog>().AsSingle();
            Container.BindInterfacesAndSelfTo<Code.Hero.Data.HeroDataService>().AsSingle();
            Container.BindInterfacesAndSelfTo<CharacterMenuService>().AsSingle();
            BindLogDataService();
            BindBlackboardService();
            BindAdsService();
            BindAssetProvider();
            BindStaticData();
            BindInputService();
            BindInputHandlerService();
            BindRandom();
            BindPersistentProgress();
            BindIAPService();
            BindWindowService();
            BindLevelTransfer();
            BindReload();
            Container.BindInterfacesAndSelfTo<ProgressRegistry>().AsSingle();
            Container.Bind<ISaveEnvironment>().FromMethod(_ => SaveEnvironment.Current).AsSingle();
            BindSaveLoad();
            BindPoolService();
            Container.Bind<WorldAssetFactory>().AsSingle();
            Container.Bind<IHeroFactory>().To<HeroFactory>().AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();
            Container.Bind<ILootFactory>().To<LootFactory>().AsSingle();
            BindGameFactory();
            BindUIFactory();
        }

        private void BindLogDataService() =>
            Container.BindInterfacesTo<LogDataService>()
                .AsSingle()
                .NonLazy();

        private void BindBlackboardService() =>
            Container.Bind<IBlackboardService>()
                     .To<BlackboardService>()
                     .AsSingle()
                     .NonLazy();

        private void BindAdsService() =>
            Container.BindInterfacesTo<AdsService>()
                     .AsSingle()
                     .NonLazy();

        private void BindAssetProvider() => 
            Container.BindInterfacesTo<AssetProvider>()
                     .AsSingle()
                     .NonLazy();

        private void BindStaticData() => 
            Container.BindInterfacesTo<StaticDataService>()
                     .AsSingle()
                     .NonLazy();

        private void BindInputService() =>
            Container.BindInterfacesTo<InputService>()
                     .AsSingle()
                     .NonLazy();

        private void BindInputHandlerService() =>
            Container.Bind<IInputHandlerService>()
                     .To<InputHandlerService>()
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

        private void BindIAPService() =>
            Container.BindInterfacesTo<IAPService>()
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

        private void BindPoolService() =>
            Container.BindInterfacesTo<PoolService>()
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
            Container.Bind<MainMenuState>().AsSingle().NonLazy();
            Container.Bind<BootstrapState>().AsSingle().NonLazy();
            Container.Bind<WarmUpState>().AsSingle().NonLazy();
            Container.Bind<LoadProgressState>().AsSingle().NonLazy();
            Container.Bind<LoadLevelState>().AsSingle().NonLazy();
            Container.Bind<GameLoopState>().AsSingle().NonLazy();
            Container.Bind<ReloadGameState>().AsSingle().NonLazy();
        }

        private void BindGame() =>
            Container.BindInterfacesTo<Game>()
                     .AsSingle()
                     .NonLazy();
    }
}
