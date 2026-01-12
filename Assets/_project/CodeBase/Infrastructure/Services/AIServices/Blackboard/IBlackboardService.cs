using CodeBase.Infrastructure.Services.AIServices.ArbiterLogic;
namespace CodeBase.Infrastructure.Services.AIServices.BlackboardSystem
{
    public interface IBlackboardService : IService, IRegisterExpert
    {
        Blackboard GetBlackboard();
    }
}
