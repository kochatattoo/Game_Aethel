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
            GameBootstrapper gameBootstrapper =Container
                .InstantiatePrefabForComponent<GameBootstrapper>(GameBootstrapper);

            Container
                .Bind<ICoroutineRunner>()
                .FromInstance(gameBootstrapper)
                .AsSingle()
                .NonLazy();

            Container.Bind<SceneLoader>()
                .AsSingle()
                .NonLazy();

            LoadingCurtain loadingCurtain = Container
                .InstantiatePrefabForComponent<LoadingCurtain>(LoadingCurtain);

            Container
                .Bind<LoadingCurtain>()
                .FromInstance(loadingCurtain)
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesTo<NewInputService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<GameStateMachine>()
                .AsSingle()
                .NonLazy();

            Container.Bind<Game>()
                .AsSingle()
                .NonLazy();
        }
    }
}