using UnityEngine;

namespace CodeBase.Configs
{
    [CreateAssetMenu(fileName = nameof(AttackConfig), menuName = "Settings/" + nameof(AttackConfig))]
    public class AttackConfig : ScriptableObject
    {
        [Header("Settings")]
        [field: SerializeField]
        public LayerMask LayerName { get; private set; }

        [field: SerializeField]
        public float Radius { get; private set; } = 0.3f;

        [field: SerializeField]
        public float BladeLength { get; private set; } = 0.3f; // Длина лезвия

        [field: SerializeField]
        public Vector3 Offset = Vector3.zero; // Если нужно сместить начало

        [Header("Debug")]
        [field: SerializeField]
        public bool DrawGizmos { get; private set; } = true;

        [field: SerializeField]
        public Color GizmoColor { get; private set; } = Color.red;
    }
}
