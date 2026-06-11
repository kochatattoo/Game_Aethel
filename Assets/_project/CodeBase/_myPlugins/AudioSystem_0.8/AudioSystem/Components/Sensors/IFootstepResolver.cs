using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Sensors
{
    /// <summary>
    /// Интерфейс системы обработки шагов.
    /// Служит связующим звеном между событиями анимации (Animation Events) и логикой определения поверхности.
    /// </summary>
    public interface IFootstepResolver
    {
        /// <summary>
        /// Отрисовка зон каста и дебаг-информации в окне Scene.
        /// </summary>
        void OnDrawGizmosSelected();

    }

    /// <summary>
    /// Обобщенный интерфейс для систем шагов, возвращающий ключ определенного типа <typeparamref name="T"/>.
    /// Используется для сопоставления шага с конкретным ресурсом (например, типом поверхности или аудио-библиотекой).
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого ключа (например, String ID или ScriptableObject).</typeparam>
    public interface IFootstepResolver<T>:IFootstepResolver
    {
        /// <summary>
        /// Определяет ключ поверхности и находит трансформ ноги, совершившей шаг.
        /// </summary>
        /// <param name="footId">Идентификатор ноги (например, 0 — левая, 1 — правая), передаваемый из анимации.</param>
        /// <param name="targetFoot">Выходной параметр: ссылка на Transform кости ноги, которая коснулась земли.</param>
        /// <returns>Результат разрешения (ключ типа <typeparamref name="T"/>), на основе которого выбирается эффект или звук.</returns>
        T GetResolvedKey(int footId, out Transform targetFoot);
    }
}
