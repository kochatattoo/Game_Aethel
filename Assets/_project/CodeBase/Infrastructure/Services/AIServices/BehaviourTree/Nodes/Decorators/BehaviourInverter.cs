using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор: всегда возвращает обратный результат.
    /// </summary>
    public class BehaviourInverter : BehaviourNode
    {
        public BehaviourInverter(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            switch (Children[0].Process())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }
    }
}
