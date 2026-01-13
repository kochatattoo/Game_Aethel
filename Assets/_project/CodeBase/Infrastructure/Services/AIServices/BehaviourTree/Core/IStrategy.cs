namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core
{
    public interface IStrategy
    {
        BehaviourNode.Status Process();
        void Reset()
        {
            // noop
        }
    }
}

