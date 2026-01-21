using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using System;


namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Leaf
{
    public class Condition : IStrategy
    {
        readonly Func<bool> predicate;

        public Condition(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public BehaviourNode.Status Process() => predicate() ? BehaviourNode.Status.Success : BehaviourNode.Status.Failure;
    }
}
