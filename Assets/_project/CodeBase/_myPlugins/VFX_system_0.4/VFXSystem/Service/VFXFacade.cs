using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.Factory;
using VFXSystem.Parameters.Definition;
using VFXSystem.Resolver;

namespace VFXSystem.Service
{
    /// <summary>
    /// Класс для вызова VFX в других классах / отклике на событие
    /// </summary>
    public class VFXFacade: IVFXFacade
    {
        // TODO: Можно почистить данный класс

        private readonly IVFXSystemService _vFXSystemService;
        private readonly IVFXFactory _vFXFactory;
        private readonly IVFXValidator<HitVFXDefinition> _compositeValidator;

        public VFXFacade(IVFXSystemService vfxSystemService, IVFXFactory vFXFactory, IVFXValidator<HitVFXDefinition> validator)
        {
            _vFXSystemService = vfxSystemService;
            _vFXFactory = vFXFactory;
            _compositeValidator = validator;
        }

        //Вызываем с помощью фабрики VFX
        public void CreateVFX(VFXPointData data)
        {
            var definition = _vFXSystemService.GetVFXfromMap(data.MaterialType);

            if (definition == null)
                return;

            Play(data, definition);
        }

        public void CreateVFX(Material material, VFXPointData data)
        {
            var definition = _vFXSystemService.GetVFXfromMap(material);

            if (definition == null)
                return;

            Play(data, definition);
        }

        public void CreateVFX(MaterialType materialType, Vector3 point, Vector3 normal, Transform target, float impactStrenght = 1)
        {
            //Надо Обработка в случае null
            var definition = _vFXSystemService.GetVFXfromMap(materialType);

            if (definition == null)
                return;

            Play(point, normal, target, impactStrenght, definition);
        }

        public void CreateVFX(Material material, Vector3 point, Vector3 normal, Transform target, float impactStrenght = 1)
        {
            var definition = _vFXSystemService.GetVFXfromMap(material);

            if (definition == null)
                return;

            Play(point, normal, target, impactStrenght, definition);
        }

        #region Private

        private void Play(VFXPointData data, HitVFXDefinition definition)
        {
            VFXSpawnContext<HitVFXDefinition> spawnContext = new VFXSpawnContext<HitVFXDefinition>
            {
                Definition = definition,
                Position = data.Position
            };

            Debug.Log($"Validator is: {_compositeValidator}");

            if (!_compositeValidator.CanSpawn(spawnContext))
                return;

            PlayVFX(data, definition);
        }

        private void Play(Vector3 point, Vector3 normal, Transform target, float impactStrenght, HitVFXDefinition definition)
        {
            VFXSpawnContext<HitVFXDefinition> spawnContext = new VFXSpawnContext<HitVFXDefinition>
            {
                Definition = definition,
                Position = point
            };

            Debug.Log($"Validator is: {_compositeValidator}");

            if (!_compositeValidator.CanSpawn(spawnContext))
                return;

            PlayVFX(point, normal, impactStrenght, target, definition);
        }

        private void PlayVFX(Vector3 point, Vector3 normal, float impactStrenght, Transform target, HitVFXDefinition definition)
        {
            var entity = _vFXFactory.CreateVFXEntity(definition);
            var rotation = Quaternion.LookRotation(normal);

            entity.PlayAt(point, rotation, target, impactStrenght);
        }

        private void PlayVFX(VFXPointData data, HitVFXDefinition definition)
        {
            var entity = _vFXFactory.CreateVFXEntity(definition);

            entity.PlayAt(data.Position, data.HitRotation, data.Parent, data.Interact);
        }
        #endregion
    }
}