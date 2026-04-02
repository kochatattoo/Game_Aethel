using Infrastructure.AudioSystem.Parameters.DTO;
using System;
using UnityEngine;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Интерфейс фабрики для управления жизненным циклом динамических аудио-объектов (AudioEntity).
    /// Отвечает за спавн из пула, позиционирование и автоматическую очистку после завершения звука.
    /// </summary>
    public interface IAudioComponentFactory: IDisposable
    {
        /// <summary> Создает аудио-объект, который будет перемещаться вместе с указанным родителем. </summary>
        /// <param name="eventName">Ключ события Wwise (строковый идентификатор).</param>
        /// <param name="parent">Трансформ-родитель, за которым закрепится звук.</param>
        void CreateAttached(AudioKey eventName, Transform parent);

        /// <summary> Создает аудио-объект, привязанный к родителю, с предварительной установкой материала или состояния (Switch). </summary>
        /// <param name="eventName">Ключ события Wwise.</param>
        /// <param name="switchName">Ключ переключателя (например, тип поверхности).</param>
        /// <param name="parent">Трансформ-родитель.</param>
        void CreateAttached(AudioKey eventName, AudioKey switchName, Transform parent);

        /// <summary> Создает аудио-объект, привязанный к родителю, с предварительной установкой числового параметра (RTPC). </summary>
        /// <param name="eventName">Ключ события Wwise.</param>
        /// <param name="paramName">Ключ параметра управления (например, скорость или громкость).</param>
        /// <param name="value">Численное значение параметра.</param>
        /// <param name="parent">Трансформ-родитель.</param>
        void CreateAttached(AudioKey eventName, AudioKey paramName, float value, Transform parent);

        /// <summary> Создает аудио-объект, привязанный к родителю, с полной контекстной настройкой (Switch и RTPC). </summary>
        /// <param name="eventName">Ключ события Wwise.</param>
        /// <param name="switchName">Ключ переключателя.</param>
        /// <param name="paramName">Ключ RTPC параметра.</param>
        /// <param name="value">Значение параметра.</param>
        /// <param name="parent">Трансформ-родитель.</param>
        void CreateAttached(AudioKey eventName, AudioKey switchName, AudioKey paramName, float value, Transform parent);

        /// <summary> Создает аудио-объект, следующий за родителем, используя прямой тип SDK Wwise. </summary>
        /// <param name="eventName">Объект события Wwise из инспектора.</param>
        /// <param name="parent">Трансформ-родитель.</param>
        void CreateAttached(AK.Wwise.Event eventName, Transform parent);

        /// <summary> Создает привязанный к родителю аудио-объект с установкой типа Wwise Switch. </summary>
        /// <param name="eventName">Объект события Wwise.</param>
        /// <param name="switchName">Объект переключателя Wwise.</param>
        /// <param name="parent">Трансформ-родитель.</param>
        void CreateAttached(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, Transform parent);

        /// <summary> Создает привязанный к родителю аудио-объект с установкой типа Wwise RTPC. </summary>
        /// <param name="eventName">Объект события Wwise.</param>
        /// <param name="paramName">Объект RTPC параметра Wwise.</param>
        /// <param name="value">Значение параметра.</param>
        /// <param name="parent">Трансформ-родитель.</param>
        void CreateAttached(AK.Wwise.Event eventName, AK.Wwise.RTPC paramName, float value, Transform parent);

        /// <summary> Создает привязанный к родителю аудио-объект с полной настройкой через типы SDK Wwise. </summary>
        /// <param name="eventName">Объект события Wwise.</param>
        /// <param name="switchName">Объект переключателя Wwise.</param>
        /// <param name="paramName">Объект RTPC параметра Wwise.</param>
        /// <param name="value">Значение параметра.</param>
        /// <param name="parent">Трансформ-родитель.</param>
        void CreateAttached(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Transform parent);

        /// <summary> Создает разовый аудио-эффект (One-Shot) в фиксированной мировой позиции. </summary>
        /// <param name="eventName">Ключ события Wwise.</param>
        /// <param name="position">Мировая позиция появления звука.</param>
        void CreateOneShot(AudioKey eventName, Vector3 position);

        /// <summary> Создает разовый аудио-эффект в позиции с предварительной установкой переключателя (Switch). </summary>
        /// <param name="eventName">Ключ события Wwise.</param>
        /// <param name="switchName">Ключ переключателя (материал поверхности и т.д.).</param>
        /// <param name="position">Мировая позиция.</param>
        void CreateOneShot(AudioKey eventName, AudioKey switchName, Vector3 position);

        /// <summary> Создает разовый аудио-эффект в позиции с предварительной установкой числового параметра (RTPC). </summary>
        /// <param name="eventName">Ключ события Wwise.</param>
        /// <param name="paramName">Ключ RTPC параметра.</param>
        /// <param name="value">Значение параметра.</param>
        /// <param name="position">Мировая позиция.</param>
        void CreateOneShot(AudioKey eventName, AudioKey paramName, float value, Vector3 position);

        /// <summary> Создает разовый аудио-эффект в позиции с полной контекстной настройкой (Switch и RTPC). </summary>
        /// <param name="eventName">Ключ события Wwise.</param>
        /// <param name="switchName">Ключ переключателя.</param>
        /// <param name="paramName">Ключ RTPC параметра.</param>
        /// <param name="value">Значение параметра.</param>
        /// <param name="position">Мировая позиция.</param>
        void CreateOneShot(AudioKey eventName, AudioKey switchName, AudioKey paramName, float value, Vector3 position);

        /// <summary> Создает разовый аудио-эффект в позиции, используя прямой тип SDK Wwise. </summary>
        /// <param name="eventName">Объект события Wwise.</param>
        /// <param name="position">Мировая позиция.</param>
        void CreateOneShot(AK.Wwise.Event eventName, Vector3 position);

        /// <summary> Создает разовый аудио-эффект в позиции с установкой типа Wwise Switch. </summary>
        /// <param name="eventName">Объект события Wwise.</param>
        /// <param name="switchName">Объект переключателя Wwise.</param>
        /// <param name="position">Мировая позиция.</param>
        void CreateOneShot(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, Vector3 position);

        /// <summary> Создает разовый аудио-эффект в позиции с установкой типа Wwise RTPC. </summary>
        /// <param name="eventName">Объект события Wwise.</param>
        /// <param name="rTPC">Объект RTPC параметра Wwise.</param>
        /// <param name="value">Значение параметра.</param>
        /// <param name="position">Мировая позиция.</param>
        void CreateOneShot(AK.Wwise.Event eventName, AK.Wwise.RTPC rTPC, float value, Vector3 position);

        /// <summary> Создает разовый аудио-эффект в позиции с полной настройкой через типы SDK Wwise. </summary>
        /// <param name="eventName">Объект события Wwise.</param>
        /// <param name="switchName">Объект переключателя Wwise.</param>
        /// <param name="paramName">Объект RTPC параметра Wwise.</param>
        /// <param name="value">Значение параметра.</param>
        /// <param name="position">Мировая позиция.</param>
        void CreateOneShot(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Vector3 position);
    }
}
