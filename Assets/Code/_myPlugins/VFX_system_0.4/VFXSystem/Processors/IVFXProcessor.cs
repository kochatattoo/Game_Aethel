using UnityEngine;
using VFXSystem.Resolver;

namespace VFXSystem.Processors
{
    public interface IVFXProcessor
    {
        /// <summary>
        /// Воспроизводит эффект на основе заранее подготовленных данных о точке контакта.
        /// </summary>
        /// <param name="data">Данные о позиции, нормали и типе поверхности для VFX.</param>
        void PlayVFX(VFXPointData data);

        /// <summary>
        /// Вычисляет и сразу воспроизводит эффект в месте столкновения.
        /// </summary>
        /// <param name="targetCollider">Коллайдер цели, с которой произошло взаимодействие.</param>
        /// <param name="bladePos">Текущая позиция лезвия/оружия.</param>
        /// <param name="impactStrenght">Сила удара (влияет на интенсивность эффекта).</param>
        void PlayVFX(Collider targetCollider, Vector3 bladePos, float impactStrenght = 1f);

        /// <summary>
        /// Определяет параметры эффекта (тип поверхности, точка хита) без его немедленного запуска.
        /// </summary>
        /// <returns>Возвращает структуру с данными для последующего воспроизведения VFX.</returns>
        VFXPointData ResolveVFX(Collider targetCollider, Vector3 bladePos, float impactStrenght = 1);
    }
}
