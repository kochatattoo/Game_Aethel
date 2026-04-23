using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.Resolver;
using static UnityEngine.GraphicsBuffer;

namespace VFXSystem.Service
{
    /// <summary>
    /// Интерфейс для вызова VFX события
    /// </summary>
    public interface IVFXFacade
    {
        /// <summary>
        /// Создание VFX'ов в точке взаимодействия
        /// </summary>
        /// <param name="materialType">Тип материала взаимодействия</param>
        /// <param name="point">Точка взаимодействия</param>
        /// <param name="normal">Нормаль взаимодействия</param>
        /// <param name="impactStrenght">Сила взаимодействия</param>
        void CreateVFX(MaterialType materialType, Vector3 point, Vector3 normal, Transform target, float impactStrenght = 1);
        void CreateVFX(Material material, Vector3 point, Vector3 normal, Transform target, float impactStrenght = 1);
        void CreateVFX(VFXPointData data);
        void CreateVFX(Material material, VFXPointData data);
    }
}