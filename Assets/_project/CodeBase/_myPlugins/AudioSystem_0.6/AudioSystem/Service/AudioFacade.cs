using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Factory;
using Infrastructure.AudioSystem.Parameters.DTO;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Infrastructure.AudioSystem
{
    /// <summary>
    /// Главный интерфейс взаимодействия с аудиосистемой. 
    /// Связывает логику воспроизведения, параметры Wwise и управление динамическими объектами.
    /// </summary>
    public class AudioFacade : IAudioFacade
    {
        private readonly IAudioService _audioService;
        private readonly IAudioComponentFactory _factory;

        public IReadOnlyReactiveProperty<float> GlobalVolume => _audioService.GlobalVolume;

        public AudioFacade(IAudioService audioService, IAudioComponentFactory factory)
        {
            _audioService = audioService;
            _factory = factory;
        }

        public void SetGlobalVolume(float volume) => _audioService.GlobalVolume.Value = volume;

        #region Factory (Билдеры)

        // Типизированные билдеры Wwise
        public AudioRequest PlayOneShot(AK.Wwise.Event wwiseEvent, Vector3 position) =>
            _factory.CreateOneShot(wwiseEvent, position);

        public AudioRequest PlayAttached(AK.Wwise.Event wwiseEvent, Transform parent) =>
            parent != null ? _factory.CreateAttached(wwiseEvent, parent) : default;

        // Билдеры для строковых ключей (AudioKey)
        public AudioKeyRequest PlayOneShot(AudioKey eventKey, Vector3 position) =>
            _factory.CreateOneShot(eventKey, position);

        public AudioKeyRequest PlayAttached(AudioKey eventKey, Transform parent) =>
            parent != null ? _factory.CreateAttached(eventKey, parent) : default;

        // Билдеры для множественных параметоров Wwise
        public AudioBatchRequest PlayBatchOneShot(AK.Wwise.Event wwiseEvent, Vector3 position) => 
            _factory.CreateBatchOneShot(wwiseEvent, position);

        public AudioBatchRequest PlayBatchAttached(AK.Wwise.Event wwiseEvent, Transform parent) => 
            parent != null ? _factory.CreateBatchAttached(wwiseEvent, parent) : default;

        // Поддержка ScriptableObject ассетов
        public AudioRequest PlayOneShot(AudioEventAsset asset, Vector3 position) =>
            PlayOneShot(asset.WwiseEvent, position);

        public AudioRequest PlayAttached(AudioEventAsset asset, Transform parent) =>
            PlayAttached(asset.WwiseEvent, parent);

        public AudioRequest PlayOneShot(AudioEventAsset asset, Vector3 position, IAudioEnviromentMaker environmentSource)
        {
            var request = PlayOneShot(asset, position);
            ApplyEnvironmentToRequest(request, environmentSource);
            return request;
        }

        public AudioRequest PlayAttached(AudioEventAsset asset, Transform parent, IAudioEnviromentMaker environmentSource)
        {
            var request = PlayAttached(asset, parent);
            ApplyEnvironmentToRequest(request, environmentSource);
            return request;
        }

        public AudioBatchRequest PlayBatchOneShot(AudioEventAsset asset, Vector3 position) =>
           PlayBatchOneShot(asset.WwiseEvent, position);

        public AudioBatchRequest PlayBatchAttached(AudioEventAsset asset, Transform parent) => 
            PlayBatchAttached(asset.WwiseEvent, parent);

        public AudioBatchRequest PlayBatchOneShot(AudioEventAsset asset, Vector3 position, IAudioEnviromentMaker environmentSource) 
        { 
            var request = PlayBatchOneShot(asset, position);
            ApplyEnvironmentToRequest(request, environmentSource);
            return request;
        }

        public AudioBatchRequest PlayBatchAttached(AudioEventAsset asset, Transform parent, IAudioEnviromentMaker environmentSource)
        {
            var request = PlayBatchAttached(asset, parent);
            ApplyEnvironmentToRequest(request, environmentSource);
            return request;
        }

        #endregion

        #region Object
        public void PostEvent(AK.Wwise.Event wwiseEvent, GameObject target) =>
            _audioService.PostEvent(wwiseEvent, target);

        public void PostEvent(AudioKey eventName, GameObject target) =>
            _audioService.PostEvent(eventName, target);

        public void StopEvent(AudioKey eventName, GameObject target) =>
            _audioService.StopPlayingEvent(eventName, target);

        public void StopEvent(AK.Wwise.Event wwiseEvent, GameObject target) =>
            _audioService.StopPlayingEvent(wwiseEvent, target);

        public void SetSwitch(AudioKey switches, GameObject target) =>
            _audioService.SetSwitch(switches, target);

        public void SetSwitch(AK.Wwise.Switch switchName, GameObject target) =>
            _audioService.SetSwitch(switchName, target);

        public void SetState(AudioKey states) =>
            _audioService.SetState(states);

        public void SetState(AK.Wwise.State state) =>
            _audioService.SetState(state);

        public void SetParameter(AudioKey paramName, float value, GameObject target = null) =>
            _audioService.SetRtpc(paramName, value, target);

        public void SetParameter(AK.Wwise.RTPC rTPC, float value, GameObject target = null) =>
            _audioService.SetRtpc(rTPC, value, target);

        public void SetParameter(AudioParameterAsset asset, float value, GameObject target = null) => 
            _audioService.SetParameter(asset, value, target);

        public void SetMultiPosition(GameObject target, List<Transform> positions, AkMultiPositionType type) =>
            _audioService.SetMultiPosition(target, positions, type);

        public void SetMultiPositionFromCache(GameObject target, AkPositionArray posArray, AkMultiPositionType type) =>
            _audioService.SetMultiPositionFromCache(target, posArray, type);

        public void ClearPositions(GameObject target) =>
            _audioService.ClearPositions(target);

        public void SetGameObjectAuxSend(GameObject target, AK.Wwise.AuxBus auxBus) =>
            _audioService.SetGameObjectAuxSend(target, auxBus);

        public void ResetGameObjectAuxSend(GameObject target) =>
            _audioService.ResetGameObjectAuxSend(target);

        public void SetBlendedAuxSends(GameObject target, AK.Wwise.AuxBus auxBusIdA, float volumeA, AK.Wwise.AuxBus auxBusIdB, float volumeB) =>
            _audioService.SetBlendedAuxSends(target, auxBusIdA, volumeA, auxBusIdB, volumeB);

        #endregion

        private void ApplyEnvironmentToRequest(AudioRequest request, IAudioEnviromentMaker source)
        {
            if (source == null) 
                return;

            var auxData = source.GetCurrentAuxSendData();
            if (auxData.IsBlended)
                request.WithBlendedAuxSends(auxData.AuxBusA, auxData.VolumeA, auxData.AuxBusB, auxData.VolumeB);
            else if (auxData.AuxBusA != null)
                request.WithBus(auxData.AuxBusA);
        }

        private void ApplyEnvironmentToRequest(AudioBatchRequest request, IAudioEnviromentMaker source)
        {
            if (source == null) 
                return;

            var auxData = source.GetCurrentAuxSendData();
            if (auxData.IsBlended)
                request.WithBlendedAuxSends(auxData.AuxBusA, auxData.VolumeA, auxData.AuxBusB, auxData.VolumeB);
            else if (auxData.AuxBusA != null)
                request.WithBus(auxData.AuxBusA);
        }
    }
}
