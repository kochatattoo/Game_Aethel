using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Composite
{
    public class BehaviourSelector : BehaviourNode
    {
        public BehaviourSelector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (_currentChild < Children.Count)
            {
                switch (Children[_currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        _currentChild++;
                        return Status.Running;
                }
            }

            Reset();
            return Status.Failure;
        }

        public override void Reset()
        {
            base.Reset();
            _currentChild = 0;
        }
    }
}
