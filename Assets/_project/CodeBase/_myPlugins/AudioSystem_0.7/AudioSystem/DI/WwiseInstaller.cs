using AudioSystem.Banks;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.WwiseSystem;
using UnityEngine;
using Zenject;

namespace Infrastructure.DI
{
    public class WwiseInstaller: MonoInstaller
    {
        [SerializeField] private WwiseEngineInitializer _wwiseInitializer;
        [SerializeField] private WwiseBankAsset _initBank; // Ссылка на Init.bnk

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<WwiseEngineInitializer>()
                .FromComponentInNewPrefab (_wwiseInitializer.gameObject)
                .AsSingle()
                .NonLazy();

            Container.Bind<WwiseBankAsset>()
                .FromInstance(_initBank)
                .WhenInjectedInto<BankLoaderService>();
        }
    }
}