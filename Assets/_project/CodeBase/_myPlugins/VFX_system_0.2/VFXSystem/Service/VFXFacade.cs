using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.Factory;
using VFXSystem.Parameters.Definition;

namespace VFXSystem.Service
{
    /// <summary>
    /// Класс для вызова VFX в других классах / отклике на событие
    /// </summary>
    public class VFXFacade: IVFXFacade
    {
        private readonly IVFXSystemService _vFXSystemService;
        private readonly IVFXFactory _vFXFactory;

        public VFXFacade(IVFXSystemService vfxSystemService, IVFXFactory vFXFactory)
        {
            _vFXSystemService = vfxSystemService;
            _vFXFactory = vFXFactory;
        }

        //Вызываем с помощью фабрики VFX
        public void CreateVFX(MaterialType materialType, Vector3 point, Vector3 normal, float impactStrenght = 1)
        {
            //Надо Обработка в случае null
            var definition = _vFXSystemService.GetVFXfromMap(materialType);
            if (definition == null)
                return;

            PlayVFX(point, normal, impactStrenght, definition);
        }

        public void CreateVFX(Material material, Vector3 point, Vector3 normal, float impactStrenght = 1)
        {
            var definition = _vFXSystemService.GetVFXfromMap(material);
            if (definition == null)
                return;

            PlayVFX(point, normal, impactStrenght, definition);
        }

        private void PlayVFX(Vector3 point, Vector3 normal, float impactStrenght, HitVFXDefinition definition)
        {
            var entity = _vFXFactory.CreateVFXEntity(definition);
            // Поворачиваем эффект «от поверхности» по нормали
            var rotation = Quaternion.LookRotation(normal);

            //Вот тут мы передаем параметры и вызываем наши VFX'ы
            entity.PlayAt(point, rotation, impactStrenght);
        }
    }
}