using CodeBase.Infrastructure;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.State;
using CodeBase.Logic;
using UnityEngine;
using Zenject;

namespace CodeBase.DI
{
    public class BootstrapInstaller : MonoInstaller
    {
        public GameObject GameBootstrapper;
        public GameObject LoadingCurtain;

        public override void InstallBindings()
        {
            BindCoroutineRunner();
            BindSceneLoader();
            BindLoadingCurtain();

            BindServices();

            BindGameStateMachine();
            BindGame();
        }

        private void BindServices()
        {
            BindInputService();
        }

        private void BindGame()
        {
            Container.Bind<Game>()
                .AsSingle()
                .NonLazy();
        }

        private void BindGameStateMachine()
        {
            Container
                .BindInterfacesTo<GameStateMachine>()
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

        private void BindSceneLoader()
        {
            Container.Bind<SceneLoader>()
                .AsSingle()
                .NonLazy();
        }

        private void BindCoroutineRunner()
        {
            GameBootstrapper gameBootstrapper = Container
                .InstantiatePrefabForComponent<GameBootstrapper>(GameBootstrapper);

            Container
                .Bind<ICoroutineRunner>()
                .FromInstance(gameBootstrapper)
                .AsSingle()
                .NonLazy();
        }
    }
}