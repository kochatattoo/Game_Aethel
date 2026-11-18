using UnityEngine;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration
{
    /// <summary>Настройки поведения, боевых параметров и ОД противника ближнего боя.</summary>
    [CreateAssetMenu(fileName = "MeleeTacticsAIConfig", menuName = "PetProject/AI/Melee Tactics")]
    public sealed class MeleeTacticsAIConfig : ScriptableObject
    {
        [SerializeField] private int _attackPriority = 100;
        [SerializeField] private int _movePriority = 50;
        [SerializeField] private int _endTurnPriority;
        [SerializeField, Min(1)] private int _maxMovementCostPerCommand = 6;
        [SerializeField, Min(1)] private int _actionsPerTurn = 6;
        [SerializeField, Min(1)] private int _attackCost = 3;
        [SerializeField, Min(1)] private int _maxHealth = 5;
        [SerializeField] private int _initiativeBonus;
        [SerializeField] private int _attackBonus = 3;
        [SerializeField, Min(0)] private int _defense = 12;
        [SerializeField, Min(1)] private int _damage = 2;

        public int AttackPriority => _attackPriority;
        public int MovePriority => _movePriority;
        public int EndTurnPriority => _endTurnPriority;
        public int MaxMovementCostPerCommand => _maxMovementCostPerCommand;
        public int ActionsPerTurn => _actionsPerTurn;
        public int AttackCost => _attackCost;
        public int MaxHealth => _maxHealth;
        public int InitiativeBonus => _initiativeBonus;
        public int AttackBonus => _attackBonus;
        public int Defense => _defense;
        public int Damage => _damage;

        private void OnValidate()
        {
            _maxMovementCostPerCommand = Mathf.Max(1, _maxMovementCostPerCommand);
            _actionsPerTurn = Mathf.Max(1, _actionsPerTurn);
            _attackCost = Mathf.Clamp(_attackCost, 1, _actionsPerTurn);
            _maxHealth = Mathf.Max(1, _maxHealth);
            _defense = Mathf.Max(0, _defense);
            _damage = Mathf.Max(1, _damage);
        }
    }
}
