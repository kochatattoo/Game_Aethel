using Infrastructure.AudioSystem.Abstractions;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Factory;
using UnityEngine;

namespace Infrastructure.AudioSystem
{
    public interface IAudioFacadeAbstr
    {
        AudioRequest PlayAttachedAbstr(IAudioEvent audio, Transform parent);
        AudioRequest PlayAttachedAbstr(IAudioEvent audio, Transform parent, IAudioEnviromentMaker environmentSource);
        AudioBatchRequest PlayBatchAttachedAbstr(IAudioEvent audio, Transform parent);
        AudioBatchRequest PlayBatchAttachedAbstr(IAudioEvent audio, Transform parent, IAudioEnviromentMaker environmentSource);
        AudioBatchRequest PlayBatchOneShotAbstr(IAudioEvent audio, Vector3 position);
        AudioBatchRequest PlayBatchOneShotAbstr(IAudioEvent audio, Vector3 position, IAudioEnviromentMaker environmentSource);
        AudioRequest PlayOneShot(IAudioEvent audio, Vector3 position, IAudioEnviromentMaker environmentSource);
        AudioRequest PlayOneShotAbstr(IAudioEvent audio, Vector3 position);
        void PostEventAbstr(IAudioEvent wwiseEvent, GameObject target);
        void SetBlendedAuxSendsAbstr(GameObject target, IAudioAuxBus auxBusIdA, float volumeA, IAudioAuxBus auxBusIdB, float volumeB);
        void SetGameObjectAuxSendAbstr(GameObject target, IAudioAuxBus auxBus);
        void SetParameterAbstr(IAudioRtpc rTPC, float value, GameObject target = null);
        void SetStateAbstr(IAudioState state);
        void SetSwitchAbstr(IAudioSwitch switchName, GameObject target);
        void StopEventAbstr(IAudioEvent wwiseEvent, GameObject target);
    }
}