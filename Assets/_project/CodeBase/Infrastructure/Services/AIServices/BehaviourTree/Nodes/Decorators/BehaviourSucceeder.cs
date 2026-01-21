using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор: всегда возвращает Success.
    /// Дочерний узел выполняется, но его результат игнорируется.
    /// </summary>
    public class BehaviourSucceeder : BehaviourNode
    {
        public BehaviourSucceeder(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (Children.Count == 0)
                return Status.Success;

            Children[0].Process(); // либо _currentChild
            return Status.Success;
        }
    }
}
