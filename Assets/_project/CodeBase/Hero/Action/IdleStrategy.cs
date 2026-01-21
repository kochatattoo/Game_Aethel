using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Hero.HeroBehaviour
{
    public class IdleStrategy : IStrategy
    {
        public BehaviourNode.Status Process()
        {
            return BehaviourNode.Status.Success;
        }
    }
}
