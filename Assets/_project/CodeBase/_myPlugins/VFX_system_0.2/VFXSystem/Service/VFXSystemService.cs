using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.Parameters;
using VFXSystem.Parameters.Definition;
using VFXSystem.Parameters.Mapping;
using VFXSystem.Parameters.MaterialMap;
using VFXSystem.Resolver;

namespace VFXSystem.Service
{
    /// <summary>
    ///  ласс сервис дл€ обработки VFX и получени€ параметров мапы
    /// </summary>
    public class VFXSystemService : IVFXSystemService
    {
        //TODO - класс должен содержать логику управлени€
        private readonly InteractResolver _interactionResolver;

        private readonly IMap<MaterialType, HitVFXDefinition> _hitMap;
        private readonly VFXToMaterialMap _vFXToMaterialMap;

        public IReadOnlyDictionary<MaterialType, HitVFXDefinition> Effects => _hitMap.Effects;
        public IReadOnlyDictionary<Material, MaterialType> Materials => _vFXToMaterialMap.Map;

        public VFXSystemService(VFXDatabase vfxData)
        {
            _hitMap =new Mapping<MaterialType, HitVFXDefinition>(vfxData.VFXDefinitionsMapConfigs);
            _vFXToMaterialMap = new VFXToMaterialMap(vfxData.VFXMapToMaterialConfigs);

            _interactionResolver = new InteractResolver(); //  ласс дл€ просчета силы взаимодействи€
        }

        [CanBeNull]
        public HitVFXDefinition GetVFXfromMap(MaterialType materialType)
        {
            //ѕолучаем VFX из мапы, а вот что дальше надо в классе € хз - продумать логику
            if (Effects.TryGetValue(materialType, out HitVFXDefinition hitVFX))
            {
                return hitVFX;
            }
            else
            {
                Debug.LogWarning($"VFX Definition for {materialType} doesn't consist in map");
                return null;
            }
        }

        [CanBeNull]
        public HitVFXDefinition GetVFXfromMap(Material material) => GetVFXfromMap(GetMaterialType(material));

        private MaterialType GetMaterialType(Material material)
        {
            if (Materials.TryGetValue(material, out MaterialType materialType))
            {
                return materialType;
            }
            else
            {
                Debug.LogWarning($"VFX Definition for {material} doesn't consist in map");
                return MaterialType.NoneDetected;
            }
        }
    }
}