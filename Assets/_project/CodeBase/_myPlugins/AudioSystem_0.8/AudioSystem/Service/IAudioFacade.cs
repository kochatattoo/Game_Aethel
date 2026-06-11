using AK.Wwise;
using Infrastructure.AudioSystem.Abstractions;
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
    /// Единая точка входа аудиосистемы (Facade). 
    /// Управляет жизненным циклом звуков через билдеры, контролирует глобальные параметры 
    /// и предоставляет прямой доступ к модификации звуковых объектов в движке Wwise.
    /// </summary>
    public interface IAudioFacade: IAudioFacadeAbstr, IAudioRegistrator
    {
        /// <summary> Глобальный уровень громкости всей системы. </summary>
        IReadOnlyReactiveProperty<float> GlobalVolume { get; }

        /// <summary> Устанавливает значение глобальной громкости. </summary>
        void SetGlobalVolume(float volume);

        /// <summary>
        /// Перемещает позицию звукового объекта в пространстве Wwise без изменения его Transform.
        /// </summary>
        void SetGameObjectPosition(GameObject target, Vector3 position, Vector3 forward, Vector3 up);

        #region Factory / Builders
        // Эти методы возвращают билдеры. Выстрел звука происходит только после вызова .Play()

        /// <summary> 
        /// Создает аудио-объект, привязанный к родителю (Attached). 
        /// Использует данные из ScriptableObject ассета. 
        /// </summary>
        AudioRequest PlayAttached(AudioEventAsset asset, Transform parent);

        /// <summary> 
        /// Создает аудио-объект, привязанный к родителю (Attached). 
        /// Использует строковые ключи (AudioKey). 
        /// </summary>
        AudioKeyRequest PlayAttached(AudioKey eventKey, Transform parent);

        /// <summary> 
        /// Создает аудио-объект, привязанный к родителю (Attached). 
        /// Использует типизированные события Wwise. 
        /// </summary>
        AudioRequest PlayAttached(AK.Wwise.Event wwiseEvent, Transform parent);

        /// <summary>
        /// Создаёт прикреплённый звук с автоматическим применением текущего окружения от указанного maker.
        /// </summary>
        AudioRequest PlayAttached(AudioEventAsset asset, Transform parent, IAudioEnviromentMaker environmentSource);

        /// <summary> 
        /// Создает аудио-объект, привязанный к родителю (Attached). 
        /// Использует типизированные события Wwise. 
        /// Для множества параметров
        /// </summary>
        AudioBatchRequest PlayBatchAttached(AK.Wwise.Event wwiseEvent, Transform parent);

        /// <summary> 
        /// Создает аудио-объект, привязанный к родителю (Attached). 
        /// Использует данные из ScriptableObject ассета. 
        /// /// Для множества параметров
        /// </summary>
        AudioBatchRequest PlayBatchAttached(AudioEventAsset asset, Transform parent);

        /// <summary>
        /// Создаёт прикреплённый звук с автоматическим применением текущего окружения от указанного maker.
        /// Для множества параметров
        /// </summary>
        AudioBatchRequest PlayBatchAttached(AudioEventAsset asset, Transform parent, IAudioEnviromentMaker environmentSource);
        
        /// <summary> 
        /// Создает аудио-объект в заданной мировой позиции (OneShot).
        /// Использует данные из ScriptableObject ассета. 
        /// </summary>
        AudioRequest PlayOneShot(AudioEventAsset asset, Vector3 position);

        /// <summary> Создает аудио-объект в заданной мировой позиции (OneShot). 
        /// Использует строковые ключи (AudioKey). 
        /// </summary>
        AudioKeyRequest PlayOneShot(AudioKey eventKey, Vector3 position);

        /// <summary> Создает аудио-объект в заданной мировой позиции (OneShot). 
        /// Использует типизированные события Wwise. 
        /// </summary>
        AudioRequest PlayOneShot(AK.Wwise.Event wwiseEvent, Vector3 position);

        /// <summary>
        /// Создаёт OneShot звук с автоматическим применением текущего окружения от указанного maker.
        /// </summary>
        AudioRequest PlayOneShot(AudioEventAsset asset, Vector3 position, IAudioEnviromentMaker environmentSource);

        /// <summary> 
        /// Создает аудио-объект в заданной мировой позиции (OneShot).
        /// Использует данные из ScriptableObject ассета. 
        /// Для множества параметров
        /// </summary
        AudioBatchRequest PlayBatchOneShot(AudioEventAsset asset, Vector3 position);

        /// <summary> Создает аудио-объект в заданной мировой позиции (OneShot). 
        /// Использует типизированные события Wwise. 
        /// Для множества параметров
        /// </summary>
        AudioBatchRequest PlayBatchOneShot(AK.Wwise.Event wwiseEvent, Vector3 position);

        /// <summary>
        /// Создаёт OneShot звук с автоматическим применением текущего окружения от указанного maker.
        /// Для множества параметров
        /// </summary>
        AudioBatchRequest PlayBatchOneShot(AudioEventAsset asset, Vector3 position, IAudioEnviromentMaker environmentSource);

        #endregion

        #region Direct Event Control
        // Прямое управление событиями на конкретных игровых объектах

        /// <summary> 
        /// Отправить событие (PostEvent) на конкретный GameObject по строковому ключу. 
        /// </summary>
        void PostEvent(AudioKey eventName, GameObject target);

        /// <summary> 
        /// Отправить событие (PostEvent) на конкретный GameObject через тип Wwise. 
        /// </summary>
        void PostEvent(AK.Wwise.Event wwiseEvent, GameObject target);

        /// <summary> 
        /// Остановить проигрывание всех экземпляров события на объекте по строковому ключу. 
        /// </summary>
        void StopEvent(AudioKey eventName, GameObject target);

        /// <summary>
        /// Остановить проигрывание всех экземпляров события на объекте через тип Wwise. 
        /// </summary>
        void StopEvent(AK.Wwise.Event wwiseEvent, GameObject target);

        #endregion

        #region Parameters & States
        // Изменение модификаторов звука (RTPC, Switches, States)

        /// <summary> 
        /// Установить значение RTPC параметра через строковый ключ. 
        /// </summary>
        void SetParameter(AudioKey paramName, float value, GameObject target = null);

        /// <summary> 
        /// Установить значение RTPC параметра через ассет. 
        /// </summary>
        void SetParameter(AudioParameterAsset asset, float value, GameObject target = null);

        /// <summary>
        /// Установить значение RTPC параметра через тип Wwise. 
        /// </summary>
        void SetParameter(RTPC rTPC, float value, GameObject target = null);

        /// <summary> 
        /// Изменить глобальное состояние (State) по строковому ключу. 
        /// </summary>
        void SetState(AudioKey states);

        /// <summary>
        /// Изменить глобальное состояние (State) через тип Wwise. 
        /// </summary>
        void SetState(State state);

        /// <summary>
        /// Установить значение переключателя (Switch) на объекте по строковому ключу. 
        /// </summary>
        void SetSwitch(AudioKey switches, GameObject target);

        /// <summary> 
        /// Установить значение переключателя (Switch) на объекте через тип Wwise.
        /// </summary>
        void SetSwitch(Switch switchName, GameObject target);

        #endregion

        #region Environment & Positioning
        // Работа с пространственным звучанием и шинами эффектов

        /// <summary>
        /// Направить звук объекта на вспомогательную шину (AuxBus) для применения эффектов окружения.
        /// </summary>
        void SetGameObjectAuxSend(GameObject target, AuxBus auxBus);

        /// <summary>
        /// Сбросить все направления на вспомогательные шины для объекта. 
        /// </summary>
        void ResetGameObjectAuxSend(GameObject target);

        /// <summary>
        /// Установить несколько позиций для одного звукового объекта (имитация распределенного источника).
        /// </summary>
        void SetMultiPosition(GameObject target, List<Transform> positions, AkMultiPositionType type);

        /// <summary>
        /// Установить позиции объекта, используя заранее подготовленный кэшированный массив позиций. 
        /// </summary>
        void SetMultiPositionFromCache(GameObject target, AkPositionArray posArray, AkMultiPositionType type);

        /// <summary>
        /// Очистить все настройки позиционирования для объекта.
        /// </summary>
        void ClearPositions(GameObject target);

        /// <summary>
        /// Устанавливает смешанные значения Aux Send для двух шин.
        /// Использует внутренний кешированный буфер.
        /// </summary>
        void SetBlendedAuxSends(GameObject target, AK.Wwise.AuxBus auxBusIdA, float volumeA, AK.Wwise.AuxBus auxBusIdB, float volumeB);

        #endregion
    }
}