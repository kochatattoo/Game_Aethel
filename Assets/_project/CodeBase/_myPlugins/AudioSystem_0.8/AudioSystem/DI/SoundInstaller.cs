using AudioSystem.Banks;
using AudioSystem.Room;
using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.Occlusions;
using Infrastructure.AudioSystem.Factory;
using Infrastructure.AudioSystem.Factory.GameComponents;
using Infrastructure.AudioSystem.Parameters;
using Infrastructure.AudioSystem.Zones;
using UnityEngine;
using Zenject;

namespace Infrastructure.DI
{
    public class SoundInstaller : MonoInstaller<SoundInstaller>
    {
        [SerializeField] private AudioDatabase _audioDatabase;

        public override void InstallBindings()
        {
            Container.Bind<IAudioZoneRegistry>().To<AudioZoneRegistry>().AsSingle().NonLazy();
            Container.Bind<AudioZoneInitializer>()
                     .FromNewComponentOnNewGameObject()
                     .AsSingle()
                     .NonLazy();

            Container.Bind<AudioStateModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<AudioService>().AsSingle().WithArguments(_audioDatabase).NonLazy();
            Container.BindInterfacesAndSelfTo<OcclusionService>().AsSingle().WithArguments(_audioDatabase).NonLazy();
            Container.Bind<IBankLoader>().To<BankLoaderService>().AsSingle();

            Container.BindMemoryPool<AudioEntity, AudioEntity.Pool>()
                     .WithInitialSize(_audioDatabase.InitialSize)
                     .FromComponentInNewPrefab(_audioDatabase.AudioEntityPrefab.gameObject)
                     .UnderTransformGroup("Audio_Pool")
                     .NonLazy();
            Container.Bind<IAudioComponentFactory>().To<AudioComponentFactory>().AsSingle();

            Container.BindInterfacesTo<AudioFacade>().AsSingle();

            Container.BindInterfacesTo<WwiseFootstepAudioProcessorFactory>()
                .AsSingle()
                .WithArguments(_audioDatabase.BootsMapConfig, _audioDatabase.SurfaceResolverConfig);

            Container.BindInterfacesAndSelfTo<IndoorRoomService>().AsSingle().WithArguments(_audioDatabase).NonLazy();
            Container.Bind<IAudioMakerStateService>().To<AudioMakerStateService>().AsSingle();
        }
    }
}