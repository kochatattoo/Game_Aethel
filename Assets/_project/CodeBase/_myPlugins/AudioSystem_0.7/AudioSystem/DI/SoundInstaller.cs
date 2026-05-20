using AudioSystem.Banks;
using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.Occlusions;
using Infrastructure.AudioSystem.Factory;
using Infrastructure.AudioSystem.Factory.GameComponents;
using Infrastructure.AudioSystem.Parameters;
using UnityEngine;
using Zenject;

namespace Infrastructure.DI
{
    public class SoundInstaller : MonoInstaller<SoundInstaller>
    {
        [SerializeField] private AudioDatabase _audioDatabase;

        public override void InstallBindings()
        {
            Container.Bind<AudioStateModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<AudioService>().AsSingle().WithArguments(_audioDatabase).NonLazy();
            Container.BindInterfacesAndSelfTo<OcclusionService>().AsSingle().NonLazy();

            Container.BindMemoryPool<AudioEntity, AudioEntity.Pool>()
                     .WithInitialSize(_audioDatabase.InitialSize)
                     .FromComponentInNewPrefab(_audioDatabase.AudioEntityPrefab.gameObject)
                     .UnderTransformGroup("Audio_Pool")
                     .NonLazy();

            Container.Bind<IAudioComponentFactory>().To<AudioComponentFactory>().AsSingle();

            Container.Bind<IBankLoader>().To<BankLoaderService>().AsSingle();

            Container.BindInterfacesTo<AudioFacade>().AsSingle();

            Container.BindInterfacesTo<WwiseFootstepAudioProcessorFactory>()
                .AsSingle()
                .WithArguments(_audioDatabase.BootsMapConfig, _audioDatabase.SurfaceResolverConfig);

            Container.Bind<IAudioMakerStateService>().To<AudioMakerStateService>().AsSingle();
        }
    }
}