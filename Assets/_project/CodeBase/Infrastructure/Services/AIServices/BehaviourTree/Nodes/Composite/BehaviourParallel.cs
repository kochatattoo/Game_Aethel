using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Composite
{
    public class BehaviourParallel : BehaviourNode
    {
        public BehaviourParallel(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            bool anyRunning = false;

            foreach (var child in Children)
            {
                var result = child.Process();

                if (result == Status.Failure)
                {
                    return Status.Failure;
                }
                if (result == Status.Running)
                {
                    anyRunning = true;
                }
            }

            return anyRunning ? Status.Running : Status.Success;
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}
