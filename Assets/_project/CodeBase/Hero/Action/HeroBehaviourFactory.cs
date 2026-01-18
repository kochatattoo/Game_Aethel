using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.BehaviourTreeExtansions;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Factories;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Composite;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Leaf;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using System;

namespace CodeBase.Hero.HeroBehaviour
{
    public class HeroBehaviourFactory : BehaviourFactory
    {
        private readonly Blackboard _blackboard;

        public HeroBehaviourFactory(Blackboard blackboard)
        {
            _blackboard = blackboard;
        }

        protected override BehaviourNode CreateRoot()
        {
            var rootSel = PrioritySelector("Main Root Selector", priority: 0)
                .Add(BuildMoveModule())
                .Add(IdleModule());

            return rootSel;
        }

        private BehaviourNode BuildMoveModule()
        {
            BehaviourSequence seqCheck = Sequence("ActivateMoveModule", priority: 10)
                .Add(Leaf("IsTargetExist", new Condition(IsTarget)))
                .Add(ChoiseTargetModule());

            return seqCheck;
        }

        private BehaviourNode ChoiseTargetModule()
        {
            BehaviourPrioritySelector pr_selector = PrioritySelector("CheckTarget")
                .Add(AttackTarget())
                .Add(MoveTarget());

            return pr_selector;
        }

        private BehaviourNode AttackTarget()
        {
            BehaviourSequence seqCheck = Sequence("IsAttackTarget?", 10);
            seqCheck
                .Add(Leaf("IsAttackTarget", new Condition(IsAttackTarget)))
                .Add(Leaf("HeroAttackStrategy", new HeroAttackStrategy()));

            return seqCheck;
        }

        private BehaviourNode MoveTarget()
        {
            BehaviourSequence seqCheck = Sequence("IMoveTarget?", 5);
            seqCheck
                .Add(Leaf("IsMoveTarget", new Condition(IsMoveTarget)))
                .Add(Leaf("HeroMoveStrategy", new HeroMoveStrategy()));

            return seqCheck;
        }

        private BehaviourNode IdleModule()
        {

            return new BehaviourLeaf("Idle state", new IdleStrategy(), priority: 0);
        }

        private bool IsTarget()
        {
            throw new NotImplementedException();
        }

        private bool IsAttackTarget()
        {
            throw new NotImplementedException();
        }

        private bool IsMoveTarget()
        {
            throw new NotImplementedException();
        }
    }
}
