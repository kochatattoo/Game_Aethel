using System.Collections.Generic;
using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.Parameters.Definition;

namespace VFXSystem.Service
{
    public interface IVFXSystemService
    {
        /// <summary>
        /// Словарь соответствия типа материала и SO определения для VFX
        /// </summary>
        IReadOnlyDictionary<MaterialType, HitVFXDefinition> Effects { get; }
        IReadOnlyDictionary<Material, MaterialType> Materials { get; }

        /// <summary>
        /// Получение SO параметра по типу материала
        /// </summary>
        /// <param name="materialType">Тип материала взаимодействия</param>
        /// <returns></returns>
        HitVFXDefinition GetVFXfromMap(MaterialType materialType);
        HitVFXDefinition GetVFXfromMap(Material material);
    }
}