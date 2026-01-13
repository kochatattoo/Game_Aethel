using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор: Зацикливает выполнение дерева
    /// </summary>
    public class BehaviourRepeater : BehaviourNode
    {
        public BehaviourRepeater(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (Children[_currentChild].Process() == Status.Running)
            {
                return Status.Running;
            }

            _currentChild = (_currentChild + 1) % Children.Count;
            return Children[_currentChild].Process() == Status.Success ? Status.Running : Status.Failure;

        }
    }
}
