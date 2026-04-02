using AK.Wwise;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Parameters.DTO;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Infrastructure.AudioSystem
{
    public interface IAudioFacade
    {
        /// <summary> 
        /// Реактивное свойство текущего уровня глобальной громкости (0..100). 
        /// </summary>
        IReadOnlyReactiveProperty<float> GlobalVolume { get; }

        /// <summary> 
        /// Устанавливает глобальный уровень громкости всей игры. 
        /// </summary>
        void SetGlobalVolume(float volume);

        /// <summary> 
        /// Отправляет событие Wwise напрямую на существующий GameObject. 
        /// Используется для объектов, не управляемых пулом аудио-сущностей.
        /// </summary>
        void PostEvent(AK.Wwise.Event wwiseEvent, GameObject target);

        /// <summary>
        /// Воспроизведение ивента на конкретном игровом объекте.
        /// Для звуков, которые должны "следовать" за объектом (шаги персонажа).
        /// </summary>
        void PostEvent(AudioKey eventName, GameObject target);

        /// <summary> 
        /// Устанавливает значение переключателя (Switch) для конкретного игрового объекта. 
        /// </summary>
        void SetSwitch(Switch switchName, GameObject target);

        /// <summary> 
        /// Устанавливает значение переключателя (Switch) для конкретного игрового объекта. 
        /// </summary>
        void SetSwitch(AudioKey switchesName, GameObject target);

        /// <summary> 
        /// Останавливает воспроизведение события на объекте с возможностью плавного затухания. 
        /// </summary>
        void StopEvent(AudioKey eventName, GameObject target);

        /// <summary> 
        /// Останавливает воспроизведение события на объекте с возможностью плавного затухания. 
        /// </summary>
        void StopEvent(AK.Wwise.Event wwiseEvent, GameObject target);

        /// <summary> 
        /// Устанавливает значение RTPC параметра. 
        /// Если target не указан, параметр применяется глобально ко всей игре. 
        /// </summary>
        void SetParameter(RTPC rTPC, float value, GameObject target = null);

        /// <summary> 
        /// Устанавливает значение RTPC параметра. 
        /// Если target не указан, параметр применяется глобально ко всей игре. 
        /// </summary>
        void SetParameter(AudioKey paramName, float value, GameObject target = null);

        /// <summary>
        /// Устанавливает глобальное состояние для всей игры.
        /// Пример: игрок под водой, идет бой или наступила ночь.
        /// </summary>
        void SetState(State state);

        /// <summary> 
        /// Устанавливает глобальное состояние (State), влияющее на микс всей игры. 
        /// </summary>
        void SetState(AudioKey statesName);

        /// <summary>
        /// Воспроизведение звука в конкретной точке через Пул (через фабрику).
        /// Идеально для ударов и разовых эффектов.
        /// </summary>
        void PlayOneShot(AudioEventAsset eventAsset, Vector3 position);

        /// <summary>
        /// Воспроизведение звука в конкретной точке через Пул (через фабрику).
        /// Идеально для ударов и разовых эффектов.
        /// </summary>
        void PlayOneShot(AudioKey eventName, Vector3 position);

        /// <summary>
        /// Воспроизведение звука с привязкой к родителю через Пул.
        /// Звук будет следовать за объектом (например, шаги).
        /// </summary>
        void PlayAttached(AudioEventAsset eventAsset, Transform parent);

        /// <summary>
        /// Воспроизведение звука с привязкой к родителю через Пул.
        /// Звук будет следовать за объектом (например, шаги).
        /// </summary>
        void PlayAttached(AudioKey eventName, Transform parent);

        /// <summary>
        /// Установка значения параметра из конфига
        /// </summary>
        void SetParameter(AudioParameterAsset asset, float value, GameObject target = null);

        /// <summary> 
        /// Создает разовый звук с принудительной установкой переключателя (Switch). 
        /// </summary>
        void PlayOneShot(AudioEventAsset eventAsset, AK.Wwise.Switch switchName, Vector3 position);

        /// <summary> 
        /// Создает разовый звук с принудительной установкой числового параметра (RTPC). 
        /// </summary>
        void PlayOneShot(AudioEventAsset eventAsset, AK.Wwise.RTPC paramName, float value, Vector3 position);

        /// <summary> 
        /// Создает разовый звук с полной контекстной настройкой (Switch + RTPC) на основе ассета.
        /// </summary>
        void PlayOneShot(AudioEventAsset eventAsset, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Vector3 position);

        /// <summary> 
        /// Создает привязанный к родителю звук с полной настройкой контекста (Switch + RTPC). 
        /// </summary>
        void PlayAttached(AudioKey eventName, AudioKey switchName, AudioKey paramName, float value, Transform parent);

        /// <summary> 
        /// Создает привязанный к родителю звук с установкой числового параметра (RTPC). 
        /// </summary>
        void PlayAttached(AudioKey eventName, AudioKey paramName, float value, Transform parent);

        /// <summary> 
        /// Создает привязанный к родителю звук с установкой переключателя (Switch). 
        /// </summary>
        void PlayAttached(AudioKey eventName, AudioKey switchName, Transform parent);

        /// <summary> 
        /// Создает разовый звук с полной настройкой контекста через ключи и настройки воспроизведения. 
        /// </summary>
        void PlayOneShot(AudioKey eventName, AudioKey switchName, AudioKey paramName, float value, Vector3 position);

        /// <summary> 
        /// Создает разовый звук с установкой числового параметра (RTPC) и настройками воспроизведения. 
        /// </summary>
        void PlayOneShot(AudioKey eventName, AudioKey paramName, float value, Vector3 position);

        /// <summary> 
        /// Создает разовый звук с установкой переключателя (Switch) и настройками воспроизведения. 
        /// </summary>
        void PlayOneShot(AudioKey eventName, AudioKey switchName, Vector3 position);

        /// <summary> 
        /// Создает привязанный к родителю звук на основе ассета с установкой переключателя (Switch). 
        /// </summary>
        void PlayAttached(AudioEventAsset eventAsset, AK.Wwise.Switch switchName, Transform parent);

        /// <summary> 
        /// Создает привязанный к родителю звук на основе ассета с установкой числового параметра (RTPC). 
        /// </summary>
        void PlayAttached(AudioEventAsset eventAsset, AK.Wwise.RTPC paramName, float value, Transform parent);

        /// <summary> 
        /// Создает привязанный к родителю звук на основе ассета с полной контекстной настройкой. 
        /// </summary>
        void PlayAttached(AudioEventAsset eventAsset, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Transform parent);

        /// <summary>
        /// Воспроизводит аудио-событие, привязанное к перемещению родительского объекта.
        /// </summary>
        /// <param name="eventName">Wwise-событие для запуска.</param>
        /// <param name="switchName">Переключатель (Switch), который будет установлен перед запуском.</param>
        /// <param name="paramName">Параметр (RTPC), значение которого нужно инициализировать.</param>
        /// <param name="value">Численное значение для указанного RTPC.</param>
        /// <param name="parent">Трансформ, за позицией которого будет следовать звук.</param>
        void PlayAttached(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Transform parent);

        /// <summary> 
        /// Воспроизводит привязанное событие с установкой RTPC. 
        /// </summary>
        void PlayAttached(AK.Wwise.Event eventName, AK.Wwise.RTPC paramName, float value, Transform parent);

        /// <summary> 
        /// Воспроизводит привязанное событие с установкой Switch. 
        /// </summary>
        void PlayAttached(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, Transform parent);

        /// <summary>
        /// Воспроизводит событие, следующее за указанным Transform. 
        /// </summary>
        void PlayAttached(AK.Wwise.Event eventName, Transform parent);

        /// <summary>
        /// Воспроизводит одиночное аудио-событие в фиксированной точке мирового пространства.
        /// </summary>
        /// <param name="eventName">Wwise-событие для запуска.</param>
        /// <param name="switchName">Контекстный переключатель (Switch) для звука.</param>
        /// <param name="paramName">Параметр (RTPC) для настройки характеристик звука.</param>
        /// <param name="value">Значение параметра RTPC.</param>
        /// <param name="position">Мировые координаты для позиционирования звука.</param>
        void PlayOneShot(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Vector3 position);

        /// <summary> 
        /// Воспроизводит OneShot в позиции с установкой RTPC.
        /// </summary>
        void PlayOneShot(AK.Wwise.Event eventName, AK.Wwise.RTPC paramName, float value, Vector3 position);

        /// <summary> 
        /// Воспроизводит OneShot в позиции с установкой Switch. 
        /// </summary>
        void PlayOneShot(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, Vector3 position);

        /// <summary> 
        /// Воспроизводит звук в указанных мировых координатах. 
        /// </summary>
        void PlayOneShot(AK.Wwise.Event eventName, Vector3 position);

        void SetMultiPosition(GameObject target, List<Transform> positions, AkMultiPositionType type);

        void ClearPositions(GameObject target);
        void SetMultiPositionFromCache(GameObject target, AkPositionArray posArray, AkMultiPositionType type);
    }
}
