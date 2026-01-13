using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор: выполняет ребёнка до первого Success
    /// </summary>
    public class BehaviourUntilSuccess : BehaviourNode
    {
        public BehaviourUntilSuccess(string name, int priority = 0) : base(name, priority) { }
        public override Status Process()
        {
            if (Children[0].Process() == Status.Success)
            {
                Reset();
                return Status.Success;
            }
            return Status.Running;
        }
    }
}
