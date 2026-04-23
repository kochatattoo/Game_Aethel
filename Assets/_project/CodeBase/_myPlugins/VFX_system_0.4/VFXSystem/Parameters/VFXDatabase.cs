using Shared.Utils.Constants;
using UnityEngine;
using VFXSystem.Factory;
using VFXSystem.Parameters.MaterialMap;
using VFXSystem.Parameters.Definition;
using VFXSystem.Parameters.Settings;
using JetBrains.Annotations;

namespace VFXSystem.Parameters
{
    [CreateAssetMenu(fileName = nameof(VFXDatabase), menuName = ScriptableObjectNames.VFXName + nameof(VFXDatabase))]
    public class VFXDatabase : ScriptableObject
    {
        [Header("Пул объектов")]
        [field: SerializeField, Tooltip("Префаб с компонентом VFXEntity")]
        public VFXEntity VFXEntityPrefab { get; private set; }

        [field: SerializeField, Tooltip("Размер пула аудио объектов")]
        public int InitialSize { get; private set; } = 10;

        [Header("Конфигурации")]
        [field: SerializeField, Tooltip("Список конфигураций VFX соответственно их настройкам качества")]
        public VFXQualitySettings VFXQualitySettings { get; private set; }

        [field: SerializeField, Tooltip("Список конфигураций VFX")]
        public VFXDefinitionsMapConfigs VFXDefinitionsMapConfigs { get; private set; }

        [field: SerializeField, Tooltip("Список соотношений типов материала к реальным материалам")]
        public VFXMapToMaterialConfigs VFXMapToMaterialConfigs { get; private set; }

        [field: SerializeField, Tooltip("Список ограничений вызова VFX")]
        public VFXRestrictionSettings VFXRestrictionSettings { get; private set; }


        [Header("Fallbacks"), CanBeNull]
        public HitVFXDefinition DefaultLowQualityEffect;
    }
}
