using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.BehaviourTreeExtansions;
using System.Linq;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Composite
{
    public class BehaviourRandomSelector : BehaviourPrioritySelector
    {
        protected override List<IBehaviourNode> SortChildren() => Children.Shuffle().ToList();

        public BehaviourRandomSelector(string name, int priority = 0) : base(name, priority) { }

    }
}
