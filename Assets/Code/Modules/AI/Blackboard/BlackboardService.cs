using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;

namespace CodeBase.Infrastructure.Services.AIServices.BlackboardSystem
{
    public class BlackboardService : IBlackboardService
    {
        private readonly Blackboard blackboard = new Blackboard();
        
        public Blackboard Blackboard => blackboard;
    }
}
