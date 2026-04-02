using Infrastructure.AudioSystem.WwiseSystem;
using UnityEngine;
using Zenject;

namespace Infrastructure.DI
{
    public class WwiseInstaller: MonoInstaller
    {
        [SerializeField] private WwiseEngineInitializer _wwiseInitializer; 

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<WwiseEngineInitializer>()
                .FromComponentInNewPrefab (_wwiseInitializer.gameObject)
                .AsSingle()
                .NonLazy();
        }
    }
}