using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.BehaviourTreeExtansions;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Factories;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Leaf;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;

namespace CodeBase.Hero.HeroBehaviour
{
    public class HeroBehaviourFactory : BehaviourFactory
    {
        private const string CURENT_TARGET = "CurrentTarget";

        private readonly Blackboard _blackboard;
        private readonly HeroPathFollower _heroPathFollower;
        private readonly HeroAttack _heroAttack;
        private readonly BlackboardKey _currentTarget;

        public HeroBehaviourFactory(Blackboard blackboard, HeroPathFollower follower, HeroAttack attack)
        {
            _blackboard = blackboard;
            _heroPathFollower = follower;
            _heroAttack = attack;

            _currentTarget = _blackboard.GetOrRegisterKey(CURENT_TARGET);
        }

        protected override BehaviourNode CreateRoot()
        {
            return PrioritySelector("HeroRoot", 0)
                .Add(MoveSequence())
                .Add(AttackSequence())
                .Add(InteractSequence())
                .Add(StopSequence())
                .Add(new BehaviourLeaf("Idle", new IdleStrategy(), priority: -1));
        }

        private BehaviourNode MoveSequence()
        {
            return Sequence("Move", priority: 10)
                .Add(Leaf("HasMoveTarget", new Condition(IsMoveTarget)))
                .Add(Leaf("HeroMoveStrategy", new HeroMoveStrategy(_heroPathFollower, _blackboard)));
        }

        private BehaviourNode AttackSequence()
        {
            return Sequence("Attack", priority: 20)
                .Add(Leaf("HasAttackTarget", new Condition(IsAttackTarget)))
                .Add(Leaf("HeroAttackStrategy", new HeroAttackStrategy(_blackboard, _heroPathFollower, _heroAttack)));
        }

        private BehaviourNode InteractSequence()
        {
            return Sequence("Interact", priority: 15)
                .Add(Leaf("HasInteractTarget", new Condition(IsInteractTarget)))
                .Add(Leaf("HeroInteractStrategy", new HeroInteractStrategy(_blackboard, _heroPathFollower)));
        }

        private BehaviourNode StopSequence()
        {
            return Sequence("Stop", priority: 0)
                .Add(Leaf("Stop moving", new Condition(IsStopTarget)))
                .Add(new BehaviourLeaf("Idle", new IdleStrategy()));
        }

        private bool IsMoveTarget() => 
            _blackboard.TryGetValue(_currentTarget, out TargetData targetData) && targetData?.Type == TargetType.Move;

        private bool IsAttackTarget() =>
            _blackboard.TryGetValue(_currentTarget, out TargetData targetData) && targetData?.Type == TargetType.Attack;

        private bool IsInteractTarget() =>
             _blackboard.TryGetValue(_currentTarget, out TargetData targetData) && targetData?.Type == TargetType.Interact;

        private bool IsStopTarget() =>
            !_blackboard.TryGetValue(_currentTarget, out TargetData targetData);

    }
}
