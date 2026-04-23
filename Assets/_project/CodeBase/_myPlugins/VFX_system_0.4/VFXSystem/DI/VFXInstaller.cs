using UnityEngine;
using VFXSystem.Parameters;
using VFXSystem.Service;
using Zenject;
using VFXSystem.Factory;
using VFXSystem.Resolver;
using VFXSystem.Parameters.Definition;
using VFXSystem.Quality;

namespace Infrastructure.DI
{
    public class VFXInstaller : MonoInstaller<VFXInstaller>
    {
        [SerializeField]
        private VFXDatabase _database;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<VFXSettingsProvider>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IVFXSystemService>()
                .To<VFXSystemService>()
                .AsSingle()
                .WithArguments(_database)
            .NonLazy();

            Container.Bind<VFXCooldownValidator<HitVFXDefinition>>()
                .AsSingle()
                .WithArguments(_database.VFXRestrictionSettings);
            Container.Bind<VFXCountValidator<HitVFXDefinition>>()
                .AsSingle()
                .WithArguments(_database.VFXRestrictionSettings);
            Container.Bind<VFXDistanceValidator<HitVFXDefinition>>()
                .AsSingle()
                .WithArguments(_database.VFXRestrictionSettings);
            Container.Bind<VFXFrustumValidator<HitVFXDefinition>>()
                .AsSingle(); 
            Container.Bind<VFXPerformanceValidator<HitVFXDefinition>>()
                .AsSingle()
                .WithArguments(_database.VFXRestrictionSettings);

            Container.BindInterfacesTo<VFXCompositeValidator<HitVFXDefinition>>()
                .AsSingle()
                .NonLazy();

            Container.BindMemoryPool<VFXEntity, VFXEntity.Pool>()
                .WithInitialSize(_database.InitialSize)
                .FromComponentInNewPrefab(_database.VFXEntityPrefab.gameObject)
                .UnderTransformGroup("VFX_Pool")
                .NonLazy();

            Container.Bind<IVFXFactory>().To<VFXFactory>().AsSingle();

            Container.BindInterfacesTo<VFXFacade>().AsSingle();
        }
    }
}
