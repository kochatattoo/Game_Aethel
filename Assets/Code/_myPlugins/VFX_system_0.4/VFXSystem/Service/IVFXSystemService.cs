using System.Collections.Generic;
using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.Parameters.Definition;

namespace VFXSystem.Service
{
    public interface IVFXSystemService
    {
        /// <summary>
        /// Получение SO параметра по типу материала
        /// </summary>
        /// <param name="materialType">Тип материала взаимодействия</param>
        /// <returns></returns>
        HitVFXDefinition GetVFXfromMap(MaterialType materialType);
        HitVFXDefinition GetVFXfromMap(Material material);
    }
}