using AudioSystem.Components.EquipmentConfigs;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.MaterialConfigs;
using Infrastructure.AudioSystem.Components.Processors;
using Infrastructure.AudioSystem.Components.Sensors;
using UnityEngine;

namespace Infrastructure.AudioSystem.Factory.GameComponents
{
    public class WwiseFootstepAudioProcessorFactory: IFootstepAudioProcessorFactory
    {
        private readonly IAudioFacade _audioFacade;
        private readonly SurfaceWwiseSwitchResolverConfig _surfaceConfig;
        private readonly BootsMapConfig _bootsConfig;

        public WwiseFootstepAudioProcessorFactory(
            IAudioFacade audioFacade,
            SurfaceWwiseSwitchResolverConfig surfaceConfig,
            BootsMapConfig bootsConfig)
        {
            _audioFacade = audioFacade;
            _surfaceConfig = surfaceConfig;
            _bootsConfig = bootsConfig;
        }

        public IFootstepAudioProcessor Create(
            IAudioEnviromentMaker enviromentMaker,
            IPhysicAudioMaker physicMaker,
            IEquipmentAudioMaker<BootsType> equipmentMaker,
            Transform transform,
            Transform leftFoot,
            Transform rightFoot)
        {
            var data = new FootstepResolverData<AK.Wwise.Switch>(transform, leftFoot, rightFoot, _surfaceConfig);
            return new FootstepSwitchAudioProcessor(
                _audioFacade,
                enviromentMaker,
                data,
                physicMaker,
                equipmentMaker,
                _bootsConfig);
        }
    }
}
