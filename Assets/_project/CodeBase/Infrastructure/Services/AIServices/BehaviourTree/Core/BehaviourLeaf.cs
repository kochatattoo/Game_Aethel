namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core
{
    public class BehaviourLeaf : BehaviourNode
    {
        readonly IStrategy strategy;
        public BehaviourLeaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            this.strategy = strategy;
        }

        public override Status Process() => strategy.Process();

        public override void Reset() => strategy.Reset();
    }
}

