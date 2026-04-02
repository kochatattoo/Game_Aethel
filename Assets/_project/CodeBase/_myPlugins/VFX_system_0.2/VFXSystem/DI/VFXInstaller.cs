using UnityEngine;
using VFXSystem.Parameters;
using VFXSystem.Service;
using Zenject;
using VFXSystem.Factory;

namespace Infrastructure.DI
{
    public class VFXInstaller : MonoInstaller<VFXInstaller>
    {
        [SerializeField]
        private VFXDatabase _database;

        public override void InstallBindings()
        {
            Container.Bind<IVFXSystemService>()
                .To<VFXSystemService>()
                .AsSingle()
                .WithArguments(_database)
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
