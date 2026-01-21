using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;

namespace CodeBase.Infrastructure.Services.AIServices.ArbiterLogic
{
    public interface IExpert
    {
        int GetInsistence(Blackboard blackboard);
        void Execute(Blackboard blackboard);
    }
}
