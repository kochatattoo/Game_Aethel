using CodeBase.Components.Audio;
using CodeBase.Infrastructure.Services.Audio;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.DI
{
    [CreateAssetMenu(fileName = nameof(AudioInstaller), menuName = "Installers/" +nameof(AudioInstaller))]
    public class AudioInstaller : ScriptableObjectInstaller<AudioInstaller>
    {
        [SerializeField] private AudioPoolItem _audioSourcePrefab;

        public override void InstallBindings()
        {
            Container.BindMemoryPool<AudioPoolItem, AudioPoolItem.Pool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_audioSourcePrefab)
                .UnderTransformGroup("AudioPool_Global");

            Container.Bind<IAudioService>().To<AudioService>().AsSingle();
        }
    }
}
