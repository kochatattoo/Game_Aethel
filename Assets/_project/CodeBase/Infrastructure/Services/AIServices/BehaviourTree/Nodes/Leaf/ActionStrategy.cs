using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using System;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Leaf
{
    public class ActionStrategy : IStrategy
    {
        readonly Action doSomething;

        public ActionStrategy(Action doSomething)
        {
            this.doSomething = doSomething;
        }

        public BehaviourNode.Status Process()
        {
            doSomething();
            return BehaviourNode.Status.Success;
        }
    }
}
