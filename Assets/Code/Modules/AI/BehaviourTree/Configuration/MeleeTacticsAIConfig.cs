using UnityEngine;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration
{
    /// <summary>Настройки BehaviourTree ближнего боя: приоритеты веток и лимит одного перемещения.</summary>
    [CreateAssetMenu(fileName = "MeleeTacticsAIConfig", menuName = "PetProject/AI/Melee Tactics")]
    public sealed class MeleeTacticsAIConfig : ScriptableObject
    {
        [SerializeField] private int _attackPriority = 100;
        [SerializeField] private int _movePriority = 50;
        [SerializeField] private int _endTurnPriority;
        [SerializeField, Min(1)] private int _maxMovementCostPerCommand = 6;

        public int AttackPriority => _attackPriority;
        public int MovePriority => _movePriority;
        public int EndTurnPriority => _endTurnPriority;
        public int MaxMovementCostPerCommand => _maxMovementCostPerCommand;

        private void OnValidate()
        {
            _maxMovementCostPerCommand = Mathf.Max(1, _maxMovementCostPerCommand);
        }
    }
}
