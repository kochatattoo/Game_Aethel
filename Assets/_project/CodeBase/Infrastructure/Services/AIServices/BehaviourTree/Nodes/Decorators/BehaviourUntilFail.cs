using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор: выполняет ребёнка до первого Failure, пока он не провалится – возвращает Running.
    /// По провалу сбрасывает состояние и возвращает Failure.
    /// </summary>
    public class BehaviourUntilFail : BehaviourNode
    {
        public BehaviourUntilFail(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (Children.Count == 0)
                return Status.Running;

            if (Children[0].Process() == Status.Failure)
            {
                Reset();
                return Status.Failure;
            }
            return Status.Running;
        }
    }
}
