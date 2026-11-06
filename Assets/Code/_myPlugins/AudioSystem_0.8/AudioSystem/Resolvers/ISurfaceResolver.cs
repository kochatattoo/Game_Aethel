using UnityEngine;

namespace Infrastructure.AudioSystem.Resolvers
{
    public interface ISurfaceResolver<T>
    {
        /// <summary> Текстовое описание причины выбора последней поверхности (для отладки). </summary> 
        string LastDebugReason { get; }

        /// <summary> Последний успешно определенный ключ поверхности. </summary>
        T LastDetectedSurface { get; }

        /// <summary>
        /// Выполняет проверку поверхности в массиве RaycastHit.
        /// </summary>
        /// <param name="raycastHits">Массив полученных Hit react'ов.</param>
        /// <returns>Ключ Wwise Switch - T для установки аудио-материала.</returns>
        T Resolve(RaycastHit[] raycastHits);

        /// <summary>
        /// Поиск ключа поверхности из массива
        /// </summary>
        /// <param name="raycastHits">Массив полученных Hit react'ов</param>
        /// <param name="resolve">Wwise Switch - T для установки аудио-материала.</param>
        /// <returns>Успешное получение ключа/ отсутствие ключа.</returns>
        bool TryResolve(RaycastHit[] raycastHits, ref T resolve);
    }
}