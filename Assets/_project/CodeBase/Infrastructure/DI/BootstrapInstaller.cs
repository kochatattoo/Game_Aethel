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
        public GameObject CoroutineRunner;
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
            BindInputService();
        }

        private void BindInputService()
        {
            Container
               .BindInterfacesTo<NewInputService>()
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

        private void BindGame()
        {
            Container.Bind<Game>()
                .AsSingle()
                .NonLazy();
        }
    }
}