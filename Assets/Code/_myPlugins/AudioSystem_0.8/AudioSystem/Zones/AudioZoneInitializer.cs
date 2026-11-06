using UnityEngine;
using Zenject;


namespace Infrastructure.AudioSystem.Zones
{
    public class AudioZoneInitializer : MonoBehaviour
    {
        private IAudioZoneRegistry _registry;

        [Inject]
        private void Construct(IAudioZoneRegistry registry) =>
            _registry = registry;

        private void Start() =>
            _registry.MarkReady();
    }
}
