using UnityEngine;

namespace CodeBase.Configs
{
    [CreateAssetMenu(fileName = nameof(HeroAttackConfig), menuName = "Settings/" + nameof(HeroAttackConfig))]
    public class HeroAttackConfig : ScriptableObject
    {
        [Header("Settings")]
        [field: SerializeField]
        public float AttackRange { get; private set; } = 3f;

        [field: SerializeField]
        public float AttackCooldown { get; private set; } = 1f;

    }
}
