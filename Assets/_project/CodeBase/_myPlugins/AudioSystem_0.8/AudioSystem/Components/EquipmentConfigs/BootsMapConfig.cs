using Shared.Utils.Constants;
using UnityEngine;

namespace AudioSystem.Components.EquipmentConfigs
{
    [CreateAssetMenu(fileName = nameof(BootsMapConfig), menuName = ScriptableObjectNames.AudioMaping + nameof(BootsMapConfig))]
    public class BootsMapConfig : EquipmentMapConfig<BootsType>
    {

    }
}
