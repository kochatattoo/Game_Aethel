using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using System;


namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Leaf
{
    public class WaitForCondition : IStrategy
    {
        private readonly Func<bool> _predicate;

        public WaitForCondition(Func<bool> predicate)
        {
            _predicate = predicate;
        }

        public BehaviourNode.Status Process()
        {
            return _predicate() ? BehaviourNode.Status.Success : BehaviourNode.Status.Running;
        }
    }
}
