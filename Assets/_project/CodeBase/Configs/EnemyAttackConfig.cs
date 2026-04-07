using CodeBase.Enemies;
using CodeBase.Hero;
using UnityEngine;

namespace CodeBase.Configs
{
    [CreateAssetMenu(fileName = nameof(EnemyAttackConfig), menuName = "Settings/" + nameof(EnemyAttackConfig))]
    public class EnemyAttackConfig : ScriptableObject
    {
        [Header("Settings")]
        [field: SerializeField]
        public float AttackCooldown { get; private set; } = 3f;
        [field: SerializeField]
        public float Damage { get; private set; } = 10;
        [field: SerializeField]
        public float Radius { get; private set; } = 1f;
        [field: SerializeField]
        public float EffectiveDistance { get; private set; } = 1f;
    }
}
