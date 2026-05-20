using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.Parameters;
using VFXSystem.Parameters.Definition;
using VFXSystem.Parameters.Mapping;
using VFXSystem.Parameters.MaterialMap;
using VFXSystem.Quality;

namespace VFXSystem.Service
{
    /// <summary>
    /// Класс сервис для обработки VFX и получения параметров мапы
    /// </summary>
    public class VFXSystemService : IVFXSystemService
    {
        //TODO - класс должен содержать логику управления
        private readonly IVFXSettingsProvider _settingsProvider;
        private readonly VFXToMaterialMap _vFXToMaterialMap;
        private readonly VFXDatabase _vfxData;

        private readonly Dictionary<VFXQuality, IMap<MaterialType, HitVFXDefinition>> _qualityToEffectsMap = new();
        private readonly Dictionary<MaterialType, HitVFXDefinition> _runtimeCache = new();
        private VFXQuality _lastQuality;

        public IReadOnlyDictionary<Material, MaterialType> Materials => _vFXToMaterialMap.Map;

        public VFXSystemService(VFXDatabase vfxData, IVFXSettingsProvider vFXSettingsProvider)
        {
            _vfxData = vfxData;
            _settingsProvider = vFXSettingsProvider;
            _vFXToMaterialMap = new VFXToMaterialMap(vfxData.VFXMapToMaterialConfigs);

            var definitionsMapConfigs = new Mapping<VFXQuality, VFXDefinitionsMapConfigs>(vfxData.VFXQualitySettings);

            foreach (var def in definitionsMapConfigs.Effects)
            {
                if(def.Value == null)
                    continue;

                _qualityToEffectsMap.Add(def.Key, new Mapping<MaterialType, HitVFXDefinition>(def.Value));
            }

            _lastQuality = _settingsProvider.CurrentQuality;
        }

        private MaterialType GetMaterialType(Material material)
        {
            if (material != null && _vFXToMaterialMap.TryGetMaterialType(material, out MaterialType materialType))
                return materialType;

            return MaterialType.None;

        }

        [CanBeNull]
        public HitVFXDefinition GetVFXfromMap(MaterialType materialType)
        {
            if (materialType == MaterialType.None) 
                return null;

            VFXQuality currentQuality = _settingsProvider.CurrentQuality;

            if (_lastQuality != currentQuality)
            {
                _runtimeCache.Clear();
                _lastQuality = currentQuality;
            }

            if (_runtimeCache.TryGetValue(materialType, out var cachedVfx))
                return cachedVfx;

            var resultVfx = FindBestVFX(materialType, currentQuality);

            _runtimeCache[materialType] = resultVfx;
            return resultVfx;
        }

        [CanBeNull]
        public HitVFXDefinition GetVFXfromMap(Material material) =>
            GetVFXfromMap(GetMaterialType(material));


        private HitVFXDefinition FindBestVFX(MaterialType matType, VFXQuality targetQuality)
        {
            for (int i = (int)targetQuality; i >= 0; i--)
            {
                VFXQuality checkLevel = (VFXQuality)i;
                if (_qualityToEffectsMap.TryGetValue(checkLevel, out var map))
                {
                    if (map.Effects.TryGetValue(matType, out var vfx))
                        return vfx;
                }
            }
            return _vfxData.VFXQualitySettings.DefaultLowQualityEffect;
        }
    }
}