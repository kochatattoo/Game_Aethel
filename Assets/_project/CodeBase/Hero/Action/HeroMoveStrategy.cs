using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using UnityEngine;

namespace CodeBase.Hero.HeroBehaviour
{
    public class HeroMoveStrategy : IStrategy
    {
        private readonly HeroPathFollower _heroPathFollower;
        private readonly Blackboard _blackboard;

        private bool _hasStarted = false;

        public HeroMoveStrategy(HeroPathFollower follower, Blackboard blackboard)
        {
            _heroPathFollower = follower;
            _blackboard = blackboard;
        }

        public BehaviourNode.Status Process()
        {
            Debug.Log("Hero Move Stratey Process");

            BlackboardKey currentTarget = _blackboard.GetOrRegisterKey("CurrentTarget");
            if (!_blackboard.TryGetValue(currentTarget, out TargetData targetData) || targetData.Type != TargetType.Move)
                return BehaviourNode.Status.Failure;

            if (!_hasStarted)
            {
                _heroPathFollower.MoveTo(targetData.Position);
                _hasStarted = true;
            }

            return _heroPathFollower.IsMove
                ? BehaviourNode.Status.Running
                : BehaviourNode.Status.Success;
        }

        public void Reset()
        {
            _hasStarted = false;
            _heroPathFollower.Stop();
        }
    }
}
