using CodeBase.Infrastructure.Services.AIServices.ArbiterLogic;
namespace CodeBase.Infrastructure.Services.AIServices.BlackboardSystem
{
    public interface IArbiterBlackboardService : IService, IRegisterExpert
    {
        Blackboard GetBlackboard();
    }
}
