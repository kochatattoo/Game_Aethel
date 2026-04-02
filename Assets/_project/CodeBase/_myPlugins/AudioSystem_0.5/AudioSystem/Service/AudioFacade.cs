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

        #region Factory

        public void PlayOneShot(AK.Wwise.Event eventName, Vector3 position) => 
            _factory.CreateOneShot(eventName, position);

        public void PlayOneShot(AudioKey eventName, Vector3 position) => 
            _factory.CreateOneShot(eventName, position);

        public void PlayOneShot(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, Vector3 position) => 
            _factory.CreateOneShot(eventName, switchName, position);

        public void PlayOneShot(AudioKey eventName, AudioKey switchName, Vector3 position) => 
            _factory.CreateOneShot(eventName, switchName, position);

        public void PlayOneShot(AK.Wwise.Event eventName, AK.Wwise.RTPC paramName, float value, Vector3 position) => 
            _factory.CreateOneShot(eventName, paramName, value, position);

        public void PlayOneShot(AudioKey eventName, AudioKey paramName, float value, Vector3 position) => 
            _factory.CreateOneShot(eventName, paramName, value, position);

        public void PlayOneShot(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Vector3 position) => 
            _factory.CreateOneShot(eventName, switchName, paramName, value, position);

        public void PlayOneShot(AudioKey eventName, AudioKey switchName, AudioKey paramName, float value, Vector3 position) =>
            _factory.CreateOneShot(eventName, switchName, paramName, value, position);

        public void PlayAttached(AK.Wwise.Event eventName, Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            _factory.CreateAttached(eventName, parent);
        }

        public void PlayAttached(AudioKey eventName, Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            _factory.CreateAttached(eventName, parent);
        }

        public void PlayAttached(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            _factory.CreateAttached(eventName, switchName, parent);
        }

        public void PlayAttached(AudioKey eventName, AudioKey switchName, Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            _factory.CreateAttached(eventName, switchName, parent);
        }

        public void PlayAttached(AK.Wwise.Event eventName, AK.Wwise.RTPC paramName, float value, Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            _factory.CreateAttached(eventName, paramName, value, parent);
        }

        public void PlayAttached(AudioKey eventName, AudioKey paramName, float value, Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            _factory.CreateAttached(eventName, paramName, value, parent);
        }

        public void PlayAttached(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            _factory.CreateAttached(eventName, switchName, paramName, value, parent);
        }

        public void PlayAttached(AudioKey eventName, AudioKey switchName, AudioKey paramName, float value, Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            _factory.CreateAttached(eventName, switchName, paramName, value, parent);
        }

        public void PlayOneShot(AudioEventAsset eventAsset, Vector3 position) => 
            PlayOneShot(eventAsset.WwiseEvent, position);

        public void PlayOneShot(AudioEventAsset eventAsset, AK.Wwise.Switch switchName, Vector3 position) => 
            PlayOneShot(eventAsset.WwiseEvent, switchName, position);

        public void PlayOneShot(AudioEventAsset eventAsset, AK.Wwise.RTPC paramName, float value, Vector3 position) => 
            PlayOneShot(eventAsset.WwiseEvent, paramName, value, position);

        public void PlayOneShot(AudioEventAsset eventAsset, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Vector3 position) => 
            PlayOneShot(eventAsset.WwiseEvent, switchName, paramName, value, position);

        public void PlayAttached(AudioEventAsset eventAsset, Transform parent) => 
            PlayAttached(eventAsset.WwiseEvent, parent);

        public void PlayAttached(AudioEventAsset eventAsset, AK.Wwise.Switch switchName, Transform parent) => 
            PlayAttached(eventAsset.WwiseEvent, switchName, parent);

        public void PlayAttached(AudioEventAsset eventAsset, AK.Wwise.RTPC paramName, float value, Transform parent) => 
            PlayAttached(eventAsset.WwiseEvent, paramName, value, parent);

        public void PlayAttached(AudioEventAsset eventAsset, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Transform parent) => 
            PlayAttached(eventAsset.WwiseEvent, switchName, paramName, value, parent);

        #endregion

        #region Object
        public void PostEvent(AK.Wwise.Event wwiseEvent, GameObject target) =>
            _audioService.PostEvent(wwiseEvent, target);

        public void PostEvent(AudioKey eventName, GameObject target) => 
            _audioService.PostEvent(eventName, target);

        public void StopEvent(AudioKey eventName, GameObject target) => 
            _audioService.StopPlayingEvent(eventName, target);

        public void StopEvent(AK.Wwise.Event wwiseEvent, GameObject target) =>
            _audioService.StopPlayingEvent(wwiseEvent,target);

        public void SetSwitch(AudioKey switches, GameObject target) => 
            _audioService.SetSwitch(switches, target);

        public void SetSwitch(AK.Wwise.Switch switchName, GameObject target) =>
            _audioService.SetSwitch(switchName,target);

        public void SetState(AudioKey states) => 
            _audioService.SetState(states);

        public void SetState(AK.Wwise.State state) =>
            _audioService.SetState(state);

        public void SetParameter(AudioKey paramName, float value, GameObject target = null) =>
            _audioService.SetRtpc(paramName, value, target);

        public void SetParameter(AK.Wwise.RTPC rTPC, float value, GameObject target = null) =>
            _audioService.SetRtpc(rTPC, value, target);

        public void SetParameter(AudioParameterAsset asset, float value, GameObject target = null)
        {
            //TODO: Будет достаточно поменять в сервисе
            if (asset == null) 
                return;
            _audioService.SetParameter(asset, value, target);
        }

        public void SetMultiPosition(GameObject target, List<Transform> positions, AkMultiPositionType type) => 
            _audioService.SetMultiPosition(target, positions, type);

        public void SetMultiPositionFromCache(GameObject target, AkPositionArray posArray, AkMultiPositionType type) =>
            _audioService.SetMultiPositionFromCache(target, posArray, type);

        public void ClearPositions(GameObject target) => 
            _audioService.ClearPositions(target);

        //TODO: Сделать банк лоадер -> обращение к нему через фасад (API во внешнюю систему)

        #endregion
    }
}
