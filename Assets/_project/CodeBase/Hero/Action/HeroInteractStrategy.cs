using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using UnityEngine;

namespace CodeBase.Hero.HeroBehaviour
{
    internal class HeroInteractStrategy : IStrategy
    {
        private readonly Blackboard _blackboard;

        public HeroInteractStrategy(Blackboard blackboard)
        {
            _blackboard = blackboard;
        }

        public BehaviourNode.Status Process()
        {
            BlackboardKey currentTarget = _blackboard.GetOrRegisterKey("CurrentTarget");
            if (!_blackboard.TryGetValue(currentTarget, out TargetData targetData)
                || targetData.Type != TargetType.Interact
                || targetData.HitObject == null)
                return BehaviourNode.Status.Failure;

            // Логика взаимодействия: открыть сундук, подобрать предмет и т.п.

            Debug.Log($"Attacking {targetData.HitObject.name}");
            return BehaviourNode.Status.Success;
        }

        public void Reset()
        {

        }
    }
}