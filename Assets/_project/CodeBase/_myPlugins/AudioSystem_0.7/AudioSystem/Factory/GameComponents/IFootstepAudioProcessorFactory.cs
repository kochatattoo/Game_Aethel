using AudioSystem.Components.EquipmentConfigs;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.Processors;
using UnityEngine;

namespace Infrastructure.AudioSystem.Factory.GameComponents
{
    public interface IFootstepAudioProcessorFactory
    {
        IFootstepAudioProcessor Create(
            IAudioEnviromentMaker enviromentMaker,
            IPhysicAudioMaker physicMaker,
            IEquipmentAudioMaker<BootsType> equipmentMaker,
            Transform transform,
            Transform leftFoot,
            Transform rightFoot);
    }
}
