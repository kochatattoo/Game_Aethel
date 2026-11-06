using Infrastructure.AudioSystem.Components;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Parameters.DTO;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Infrastructure.AudioSystem
{
    /// <summary>
    /// Низкоуровневый интерфейс взаимодействия с Wwise SDK. 
    /// Инкапсулирует системные вызовы, регистрацию объектов и обработку колбэков движка.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Реактивное свойство глобальной громкости. 
        /// Позволяет внешним системам (UI) подписываться на изменения.
        /// </summary>
        FloatReactiveProperty GlobalVolume { get; }

        /// <summary>
        /// Обертка над системной константой
        /// </summary>
        uint InvalidPlayingId { get; }

        /// <summary>
        /// Устанавливает уровень общей громкости игры.
        /// Внутри реализует логику обновления GlobalVolume и передачу RTPC в Wwise.
        /// </summary>
        void SetGlobalVolume(float volume);

        /// <summary>
        /// Отправляет Wwise-событие на игровой объект без отслеживания результата.
        /// </summary>
        void PostEvent(AK.Wwise.Event wwiseEvent, GameObject target);

        /// <summary>
        /// Прямая отправка звукового события на игровой объект.
        /// Используется для звуков, привязанных к жизненному циклу конкретного GameObject.
        /// </summary>
        void PostEvent(AudioKey eventName, GameObject target);

        /// <summary>
        /// Регистрирует и запускает Wwise-событие с поддержкой колбэков.
        /// </summary>
        /// <returns>Уникальный PlayingID или 0 при ошибке.</returns>
        uint PostEvent(AK.Wwise.Event wwiseEvent, GameObject target, AkCallbackManager.EventCallback callback = null, AkCallbackType callbackType = AkCallbackType.AK_EndOfEvent);

        /// <summary>
        /// Регистрирует и запускает звуковое событие Wwise с возможностью обратного вызова (callback).
        /// </summary>
        /// <param name="eventName">Имя события, настроенное в Wwise Project.</param>
        /// <param name="target">GameObject, к которому будет привязан звук (позиционирование).</param>
        /// <param name="callback">Делегат, выполняемый при наступлении события <paramref name="callbackType"/>.</param>
        /// <param name="callbackType">Тип события Wwise, на которое нужно подписаться (по умолчанию — конец воспроизведения).</param>
        /// <returns>Возвращает уникальный PlayingID ивента или 0 (InvalidPlayingId), если запуск не удался.</returns>
        uint PostEvent(AudioKey eventName, GameObject target, AkCallbackManager.EventCallback callback, AkCallbackType callbackType = AkCallbackType.AK_EndOfEvent);

        /// <summary>
        /// Устанавливает значение RTPC через объект данных Wwise.
        /// </summary>
        /// <param name="target">Если null, параметр применяется глобально.</param>
        void SetRtpc(AK.Wwise.RTPC rTPC, float value, GameObject target = null);

        /// <summary>
        /// Установка параметра RTPC (Real-Time Parameter Control).
        /// Включает обязательную валидацию значения (Clamp 0-100).
        /// Если target == null, параметр применяется глобально ко всем звукам.
        /// </summary>
        void SetRtpc(AudioKey rtpcName, float value, GameObject target = null);

        /// <summary>
        /// Переключает аудио-контекст (Switch) для объекта через объект данных Wwise.
        /// </summary>
        void SetSwitch(AK.Wwise.Switch switchValue, GameObject target);

        /// <summary>
        /// Переключение аудио-контекста для конкретного объекта.
        /// Например, смена типа поверхности (Grass/Stone) для звуков шагов.
        /// </summary>
        void SetSwitch(AudioKey switches, GameObject target);

        /// <summary>
        /// Устанавливает глобальное состояние (State) через объект данных Wwise.
        /// </summary>
        void SetState(AK.Wwise.State state);

        /// <summary>
        /// Устанавливает глобальное состояние (State) для всей аудио-среды игры.
        /// Применяется для изменения микса (например, "InMenu", "PlayerDead", "Combat").
        /// </summary>
        void SetState(AudioKey states);

        /// <summary>
        /// Выполняет действие (Stop/Pause/Resume) над Wwise-событием с настройкой затухания.
        /// </summary>
        void ExecuteAction(AK.Wwise.Event wwiseEvent, AkActionOnEventType action, GameObject target, int fadeTime, AkCurveInterpolation curve);

        /// <summary>
        /// Выполнение немедленного действия над событием (Stop, Pause, Resume).
        /// Позволяет управлять затуханием (Fade Out) и типом кривой интерполяции.
        /// </summary>
        void ExecuteAction(AudioKey eventName, AkActionOnEventType action, GameObject target, int fadeTime, AkCurveInterpolation curve);
        
        /// <summary>
        /// Остановить конкретный запущенный звук по его уникальному PlayingID.
        /// </summary>
        void StopPlayingID(uint playingId);

        /// <summary>
        /// Устанавливаем значение параметра из соответсвующего ассета
        /// </summary>
        void SetParameter(AudioParameterAsset parameter, float value, GameObject target = null);

        /// <summary>
        /// Останавливает воспроизведение конкретного Wwise-события.
        /// </summary>
        void StopPlayingEvent(AK.Wwise.Event wwiseEvent, GameObject target);

        /// <summary>
        /// Останавливает все активные экземпляры конкретного события на указанном объекте.
        /// </summary>
        void StopPlayingEvent(AudioKey eventKey, GameObject target);

        /// <summary> 
        /// Удаляет объект из реестра Wwise. Необходимо вызывать при деактивации или уничтожении объекта. 
        /// </summary>
        void UnregisterGameObject(GameObject gameObject);

        /// <summary> 
        /// Регистрирует объект в Wwise для возможности привязки звуков к его позиции. 
        /// </summary>
        void RegisterGameObject(GameObject gameObject);

        /// <summary>
        /// Принудительно обновляет имя игрового объекта в профайлере Wwise.
        /// Используется, если имя GameObject изменилось в Runtime, чтобы в Capture Log отображалось актуальное название.
        /// </summary>
        /// <param name="gameObject">Игровой объект, имя которого нужно обновить.</param>
        void UpdateObjectName(GameObject gameObject);
        void SetMultiPosition(GameObject target, List<Transform> positions, AkMultiPositionType type);
        void ClearPositions(GameObject target);
        void SetMultiPositionFromCache(GameObject target, AkPositionArray posArray, AkMultiPositionType type);
        void SetGameObjectAuxSend(GameObject target, AK.Wwise.AuxBus auxBus);
        void ResetGameObjectAuxSend(GameObject target);
        void SetGameObjectAuxSendKey(GameObject target, AudioKey auxBus);

        /// <summary>
        /// Устанавливает смешанные значения Aux Send для двух шин.
        /// Использует внутренний кешированный буфер.
        /// </summary>
        void SetBlendedAuxSends(GameObject target, AK.Wwise.AuxBus auxBusIdA, float volumeA, AK.Wwise.AuxBus auxBusIdB, float volumeB);

        /// <summary>
        /// Регистрирует комнату в Wwise, используя данные из RoomComponent.
        /// </summary>
        void RegisterRoom(RoomComponent room);

        /// <summary>
        /// Удаляет регистрацию комнаты (выгружает из Wwise).
        /// </summary>
        void UnregisterRoom(RoomComponent room);

        /// <summary>
        /// Помещает игровой объект в указанную комнату (для Spatial Audio).
        /// </summary>
        void SetGameObjectInRoom(GameObject target, ulong roomID);

        void SetObjectObstructionAndOcclusion(GameObject emitter, GameObject listener, float obstruction, float occlusion);

        void SetObjectPosition(GameObject target, Vector3 position, Vector3 forward, Vector3 up);
        bool IsEngineInitialized();
    }
}
