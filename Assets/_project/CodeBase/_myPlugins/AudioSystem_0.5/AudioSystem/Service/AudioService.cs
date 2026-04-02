using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Utils;
using Infrastructure.AudioSystem.Parameters;
using Infrastructure.AudioSystem.Parameters.DTO;
using Infrastructure.AudioSystem.WwiseSystem.WwiseKeys;
using System;
using UniRx;
using UnityEngine;
using Zenject;
using System.Collections.Generic;

namespace Infrastructure.AudioSystem
{
    /// <summary>
    /// Реализация аудиосервиса, координирующая работу между Wwise SDK, 
    /// базой данных ассетов и моделью состояния звука.
    /// </summary>
    public class AudioService : IAudioService, IInitializable, IDisposable
    {
        private readonly AudioStateModel _model; 
        private readonly AudioDatabase _database;
        private readonly WwiseValueMaping _wwiseValueMaping;
        private readonly CompositeDisposable _disposables = new();

        // TODO: Убрать или хранить на модели
        public FloatReactiveProperty GlobalVolume { get; } = new();

        /// <summary> 
        /// Значение невалидного PlayingID, предоставляемое Wwise SDK.
        /// </summary>
        public uint InvalidPlayingId => AkUnitySoundEngine.AK_INVALID_PLAYING_ID;

        public AudioService(AudioDatabase audioDatabase,AudioStateModel model)
        {
            _database = audioDatabase;
            _model = model;
            _wwiseValueMaping = new(_database.AudioConfig);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Initialize()
        {
            GlobalVolume.Value = _database.DefaultGlobalVolume;

            GlobalVolume
                .DistinctUntilChanged()
                .Subscribe(SetGlobalVolume)
                .AddTo(_disposables);

            foreach (var asset in _database.AllParameters)
            {
                FloatReactiveProperty property = _model.GetParameterProperty(asset);

                property
                    .DistinctUntilChanged()
                    .Subscribe(value => SetRtpc(asset.WwiseParameter, value))
                    .AddTo(_disposables);

                SetRtpc(asset.WwiseParameter, asset.DefaultValue);
            }
        }

        public void SetGlobalVolume(float volume)
        {
            AK.Wwise.RTPC globalParameter = _wwiseValueMaping.GetParameter(new AudioKey(WwiseParameterKeys.GlobalVolumeRtpc));

            if (globalParameter == null || !globalParameter.IsValid())
            {
                Debug.LogWarning($"[AudioService] Parameter for {globalParameter} is not assigned in AudioDatabase!");
                return;
            }

            globalParameter.SetGlobalValue(volume);
        }

        public void PostEvent(AK.Wwise.Event wwiseEvent, GameObject target)
        {
            if (wwiseEvent == null || !wwiseEvent.IsValid())
            {
                Debug.LogWarning($"[AudioService] Event for {wwiseEvent.Name} is not assigned in AudioDatabase!");
                return;
            }

            if (!AudioValidator.IsGameObjectReady(target, $"PostEvent({wwiseEvent.Name})"))
                return;

            wwiseEvent.Post(target);
        }

        public void PostEvent(AudioKey eventKey, GameObject target)
        {
            if (eventKey == null || string.IsNullOrEmpty(eventKey.Value))
            {
                Debug.LogWarning($"[AudioService] Event for {eventKey} is not assigned in AudioDatabase!");
                return;
            }

            AK.Wwise.Event wwiseEvent = _wwiseValueMaping.GetEvent(eventKey);
            PostEvent(wwiseEvent, target);
        }

        public uint PostEvent(AK.Wwise.Event wwiseEvent, GameObject target, AkCallbackManager.EventCallback callback = null, AkCallbackType callbackType = AkCallbackType.AK_EndOfEvent)
        {
            if (wwiseEvent == null || !wwiseEvent.IsValid())
            {
                Debug.LogWarning($"[AudioService] Event for {wwiseEvent.Name} is not assigned in AudioDatabase!");
                return InvalidPlayingId;
            }

            if (!AudioValidator.IsGameObjectReady(target, $"PostEvent({wwiseEvent.Name})"))
                return InvalidPlayingId;

            return wwiseEvent.Post(target, (uint)callbackType, callback);
        }

        public uint PostEvent(AudioKey eventKey, GameObject target, AkCallbackManager.EventCallback callback = null, AkCallbackType callbackType = AkCallbackType.AK_EndOfEvent)
        {
            if (eventKey == null || string.IsNullOrEmpty(eventKey.Value))
            {
                return InvalidPlayingId;
            }

            AK.Wwise.Event wwiseEvent = _wwiseValueMaping.GetEvent(eventKey);

            return PostEvent(wwiseEvent,target, callback,callbackType);
        }
       
        public void SetRtpc(AK.Wwise.RTPC rTPC, float value, GameObject target = null)
        {
            //Если бьем по врагу и передаем силу удара — используем SetRtpc.
            if (rTPC == null || !rTPC.IsValid())
            {
                Debug.LogWarning($"[AudioService] Parameter for {rTPC.Name} is not assigned in AudioDatabase!");
                return;
            }

            float clampedValue = AudioValidator.ClampRtpc(value);
            rTPC.SetValue(target, clampedValue);
        }

        public void SetRtpc(AudioKey rtpcName, float value, GameObject target = null)
        {
            if (rtpcName == null || string.IsNullOrEmpty(rtpcName.Value))
            {
                return;
            }

            AK.Wwise.RTPC wwiseParam = _wwiseValueMaping.GetParameter(rtpcName);

           SetRtpc(wwiseParam,value,target);
        }

        public void SetParameter(AudioParameterAsset parameter, float value, GameObject target = null)
        {
            //Если меняем общую громкость в настройках
            //или «уровень опасности» на локации — используем SetParameter.

            //TODO: Расширить метод после расширения SO
            FloatReactiveProperty property = _model.GetParameterProperty(parameter);
            property.Value = value;

            SetRtpc(parameter.WwiseParameter, value, target);
        }

        public void SetSwitch(AK.Wwise.Switch switchValue, GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("[AudioService] Attempted to set Switch on a null GameObject.");
                return;
            }

            if (switchValue != null && switchValue.IsValid())
            {
                switchValue.SetValue(target);
            }
        }

        public void SetSwitch(AudioKey switchKey, GameObject target)
        {
            if (switchKey == null || string.IsNullOrEmpty(switchKey.Value))
            {
                return;
            }

            AK.Wwise.Switch switchValue = _wwiseValueMaping.GetSwitch(switchKey);

            SetSwitch(switchValue, target);
        }

        public void SetState(AK.Wwise.State state)
        {
            if (state != null && state.IsValid())
            {
                // меняет состояние глобально
                state.SetValue();
            }
        }

        public void SetState(AudioKey states)
        {
            AK.Wwise.State stateValue = _wwiseValueMaping.GetState(states);
            SetState(stateValue);
        }

        public void SetMultiPosition(GameObject target, List<Transform> positions, AkMultiPositionType type)
        {
            if (positions == null || positions.Count == 0) 
                return;

            if (!AudioValidator.IsGameObjectReady(target, "MultiPosition Update"))
                return;

            var posArray = new AkPositionArray((uint)positions.Count);
            foreach (var transform in positions)
            {
                if (transform != null)
                    posArray.Add(transform.position, transform.forward, transform.up);
            }

            AkUnitySoundEngine.SetMultiplePositions(target, posArray, (ushort)posArray.Count, type);
        }

        public void SetMultiPositionFromCache(GameObject target, AkPositionArray posArray, AkMultiPositionType type)
        {
            if (!AudioValidator.IsGameObjectReady(target, "MultiPosition Cache Update"))
                return;

            AkUnitySoundEngine.SetMultiplePositions(target, posArray, (ushort)posArray.Count, type);
        }

        public void ClearPositions(GameObject target)
        {
            if (target == null) 
                return;

            AkUnitySoundEngine.SetMultiplePositions(target, (AkPositionArray)null, 0, AkMultiPositionType.AkMultiPositionType_MultiDirections);

            AudioValidator.UnregisterObject(target);
        }

        public void ExecuteAction(AK.Wwise.Event wwiseEvent, AkActionOnEventType action, GameObject target, int fadeTime, AkCurveInterpolation curve)
        {
            if (wwiseEvent == null || !wwiseEvent.IsValid())
            {
                Debug.LogWarning($"[AudioService] Event for {wwiseEvent.Name} is not assigned in AudioDatabase!");
                return;
            }

            if (!AudioValidator.IsGameObjectReady(target, $"PostEvent({wwiseEvent.Name})"))
                return;

            wwiseEvent.ExecuteAction(target, action, fadeTime, curve);
        }

        public void ExecuteAction(AudioKey eventKey, AkActionOnEventType action, GameObject target, int fadeTime, AkCurveInterpolation curve)
        {
            AK.Wwise.Event wwiseEvent = _wwiseValueMaping.GetEvent(eventKey);

            ExecuteAction(wwiseEvent, action, target, fadeTime, curve);
        }

        public void StopPlayingID(uint playingId)
        {
            if (playingId == AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
                return;

            AkUnitySoundEngine.StopPlayingID(playingId);
        }

        public void StopPlayingEvent(AK.Wwise.Event wwiseEvent, GameObject target)
        {
            if (wwiseEvent == null || !wwiseEvent.IsValid())
            {
                Debug.LogWarning($"[AudioService] Event for {wwiseEvent.Name} is not assigned in AudioDatabase!");
                return;
            }

            if (!AudioValidator.IsGameObjectReady(target, $"PostEvent({wwiseEvent.Name})"))
                return;

            wwiseEvent.Stop(target);
        }

        public void StopPlayingEvent(AudioKey eventKey, GameObject target)
        {
            AK.Wwise.Event wwiseEvent = _wwiseValueMaping.GetEvent(eventKey);

            StopPlayingEvent(wwiseEvent, target);
        }

        public void UnregisterGameObject(GameObject gameObject)
        {
            AudioValidator.UnregisterObject(gameObject);
        }

        public void RegisterGameObject(GameObject gameObject)
        {
            AudioValidator.RegisterObject(gameObject);
        }

        public void UpdateObjectName(GameObject gameObject)
        {
            AudioValidator.UpdateObjectName(gameObject);
        }
    }
}
