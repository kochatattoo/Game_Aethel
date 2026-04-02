using UnityEngine;

namespace VFXSystem.VFXTypes
{
    /// <summary>
    /// Общий интерфейс для всех под-эффектов (частиц, декалей, света).
    /// Позволяет VFXEntity управлять жизненным циклом различных компонентов через единый интерфейс.
    /// </summary>
    public interface ISubVFX
    {
        /// <summary>
        /// Запускает воспроизведение эффекта.
        /// </summary>
        void Play(float impactScale = 1);

        /// <summary>
        /// Принудительно останавливает эффект и подготавливает его к возврату в пул.
        /// </summary>
        void Stop();

        /// <summary>
        /// Длительность работы эффекта в секундах. 
        /// Используется для определения момента деспавна всей сущности.
        /// </summary>
        float Duration { get; } 

        /// <summary>
        /// Ссылка на GameObject для инстанирования объекта VFX
        /// </summary>
        GameObject GameObject { get; }
    }
}
