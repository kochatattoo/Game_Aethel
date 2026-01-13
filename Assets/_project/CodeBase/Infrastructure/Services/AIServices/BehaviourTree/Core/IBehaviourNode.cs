namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core
{
    public interface IBehaviourNode
    {
        public int Priority { get; }

        public BehaviourNode.Status Process();

        public void AddChild(IBehaviourNode child);

        public void Reset();
    }
}

