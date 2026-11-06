using Shared.Utils.Constants;
using UnityEngine;

namespace VFXSystem.Parameters.Settings
{
    [CreateAssetMenu(fileName = nameof(VFXRestrictionSettings), menuName = ScriptableObjectNames.VFXName + "Setting/" + nameof(VFXRestrictionSettings))]
    public class VFXRestrictionSettings: ScriptableObject
    {
        [field: SerializeField]
        public int MaxCount { get; private set; } = 10;

        [field: SerializeField]
        public float MaxDurationTime { get; private set; } = 10f;

        [field: SerializeField]
        public float MaxDistance { get; private set; } = 50f;

        [field: SerializeField]
        public float MinSpawnInterval { get; private set; } = 1f;

        [field: SerializeField]
        public float CurrentFPS { get; private set; } = 60f;

        [field: SerializeField]
        public float MinFPS { get; private set; } = 15f;
    }
}
