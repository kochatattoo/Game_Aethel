using Shared.Utils.Constants;
using UnityEngine;

namespace VFXSystem.Parameters.Settings
{
    [CreateAssetMenu(fileName = nameof(VFXHitSensorSettings), menuName = ScriptableObjectVFXNames.VFXName + "Setting/" + nameof(VFXHitSensorSettings))]
    public class VFXHitSensorSettings
    {
        [field: SerializeField]
        public float NormilizedMultiplier { get; private set; } = 0.1f;

        [field: SerializeField]
        public float AddedDistance { get; private set; } = 0.2f;
    }
}
