using Shared.Utils.Constants;
using UnityEngine;

namespace AudioSystem.Components.EquipmentConfigs
{
    [CreateAssetMenu(fileName = nameof(EquipmentMapData), menuName = ScriptableObjectNames.AudioMaping + nameof(EquipmentMapData))]
    public class EquipmentMapData : ScriptableObject
    {
        [field: SerializeField]
        public BootsMapConfig BootsMapConfig { get; private set; }
    }
}
