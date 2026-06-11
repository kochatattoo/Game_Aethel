using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.MaterialConfigs
{
    /// <summary>
    /// Конфигурация расчета поверхности для аудиосистемы
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CastSettings), menuName = ScriptableObjectNames.AudioSettings + nameof(CastSettings))]
    public class CastSettings : ScriptableObject
    {
        [Header("Raycast Settings")]
        [field: SerializeField]
        public float CastRadius { get; private set; } = 0.01f;

        [field: SerializeField]
        public float CastDistance { get; private set; } = 1f;

        [field: SerializeField]
        public float VerticalOffset { get; private set; } = 0f;

        [field: SerializeField]
        public LayerMask LayerMask { get; private set; }

        [field: SerializeField]
        public int Buffer { get; private set; } = 10;

        [field: SerializeField]
        public float Min_Velocity { get; private set; } = 0.2f;
    }
}
