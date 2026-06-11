using Infrastructure.AudioSystem.Parameters.DTO;
using UnityEngine;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Интерфейс фабрики для управления жизненным циклом динамических аудио-объектов (AudioEntity).
    /// Обеспечивает извлечение объектов из пула, их пространственное позиционирование 
    /// и подготовку билдеров для настройки параметров воспроизведения.
    /// </summary>
    public interface IAudioComponentFactory
    {
        #region Attached Sounds
        // Создание звуков, которые следуют за игровым объектом

        /// <summary> 
        /// Подготавливает аудио-объект, привязанный к родителю. 
        /// Использует типизированные события Wwise.
        /// </summary>
        /// <returns> Билдер для настройки параметров. Необходимо вызвать .Play() для запуска. </returns>
        AudioRequest CreateAttached(AK.Wwise.Event ev, Transform parent);

        /// <summary> 
        /// Подготавливает аудио-объект, привязанный к родителю. 
        /// Использует строковые ключи (AudioKey).
        /// </summary>
        /// <returns> Билдер для настройки строковых параметров. Необходимо вызвать .Play() для запуска. </returns>
        AudioKeyRequest CreateAttached(AudioKey ev, Transform parent);

        /// <summary> 
        /// Подготавливает аудио-объект, привязанный к родителю. 
        /// Использует типизированные события Wwise.
        /// Использует множественные параметры
        /// </summary>
        /// <returns> Билдер для настройки параметров. Необходимо вызвать .Play() для запуска. </returns>
        AudioBatchRequest CreateBatchAttached(AK.Wwise.Event ev, Transform parent);

        #endregion

        #region OneShot Sounds
        // Создание звуков в фиксированной мировой точке

        /// <summary> 
        /// Подготавливает аудио-объект в заданной мировой позиции. 
        /// Использует типизированные события Wwise.
        /// </summary>
        /// <returns> Билдер для настройки параметров. Необходимо вызвать .Play() для запуска. </returns>
        AudioRequest CreateOneShot(AK.Wwise.Event ev, Vector3 position);

        /// <summary> 
        /// Подготавливает аудио-объект в заданной мировой позиции. 
        /// Использует строковые ключи (AudioKey).
        /// </summary>
        /// <returns> Билдер для настройки строковых параметров. Необходимо вызвать .Play() для запуска. </returns>
        AudioKeyRequest CreateOneShot(AudioKey ev, Vector3 position);

        /// <summary> 
        /// Подготавливает аудио-объект в заданной мировой позиции. 
        /// Использует типизированные события Wwise.
        /// Использует множественные параметры
        /// </summary>
        /// <returns> Билдер для настройки параметров. Необходимо вызвать .Play() для запуска. </returns>
        AudioBatchRequest CreateBatchOneShot(AK.Wwise.Event ev, Vector3 position);

        #endregion
    }
}
