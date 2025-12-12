using CodeBase.Infrastructure.State;

namespace CodeBase.Infrastructure.Services.Levels
{
    public interface ILevelTransferService : IService 
    {
        void GoTo(string levelName);
        void Resolve(IGameStateMachine gameState);
    }
}
