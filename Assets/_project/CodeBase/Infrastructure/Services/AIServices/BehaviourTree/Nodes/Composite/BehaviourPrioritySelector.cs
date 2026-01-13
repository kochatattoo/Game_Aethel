using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using System.Collections.Generic;
using System.Linq;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Composite
{
    public class BehaviourPrioritySelector : BehaviourSelector
    {
        List<IBehaviourNode> sortedChildren;
        List<IBehaviourNode> SortedChildren => sortedChildren ??= SortChildren();

        public BehaviourPrioritySelector(string name, int priority = 0) : base(name, priority) { }

        public override void Reset()
        {
            base.Reset();
            sortedChildren = null;
        }

        public override Status Process()
        {
            foreach (var child in SortedChildren)
            {
                switch (child.Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        return Status.Success;
                    default:
                        continue;
                }
            }

            return Status.Failure;
        }

        protected virtual List<IBehaviourNode> SortChildren() => Children.OrderByDescending(child => child.Priority).ToList();
    }
}
