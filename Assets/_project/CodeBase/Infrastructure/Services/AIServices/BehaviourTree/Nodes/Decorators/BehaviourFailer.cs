using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор: всегда возвращает Failure.
    /// Дочерний узел выполнится, но его результат будет выброшен.
    /// </summary>
    public class BehaviourFailer : BehaviourNode
    {
        public BehaviourFailer(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            Children[0].Process();
            return Status.Failure;
        }
    }
}
