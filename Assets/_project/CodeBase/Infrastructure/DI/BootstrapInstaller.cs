using CodeBase.Infrastructure;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Infrastructure.State;
using CodeBase.Logic;
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
            BindCoroutineRunner();
            BindSceneLoader();
            BindLoadingCurtain();

            BindServices();

            BindGameStateMachine();
        }

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

        private void BindSceneLoader()
        {
            Container.Bind<SceneLoader>()
                .AsSingle()
                .NonLazy();
        }

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
            BindAssetProvider();
            BindStaticData();
            BindInputService();
            BindGameFactory();
            BindStateFactory();
        }

        private void BindAssetProvider()
        {
            Container
                .BindInterfacesTo<AssetProvider>()
                .AsSingle()
                .NonLazy();
        }

        private void BindStaticData()
        {
            Container
               .BindInterfacesTo<StaticDataService>()
               .AsSingle()
               .NonLazy();
        }

        private void BindInputService()
        {
            Container
               .BindInterfacesTo<NewInputService>()
               .AsSingle()
               .NonLazy();
        }

        private void BindGameFactory()
        {
            Container
                .BindInterfacesTo<GameFactory>()
                .AsSingle()
                .NonLazy();
        }

        private void BindStateFactory()
        {
           Container
                .BindInterfacesTo<StateFactory>()
                .AsSingle()
                .NonLazy();
        }

        private void BindGameStateMachine()
        {
            BindStates();

            Container
                .BindInterfacesTo<GameStateMachine>()
                .AsSingle()
                .NonLazy();
        }

        private void BindStates()
        {
            Container.Bind<BootstrapState>().AsSingle().NonLazy();
            Container.Bind<LoadLevelState>().AsSingle().NonLazy();
            Container.Bind<GameLoopState>().AsSingle().NonLazy();
        }
    }
}