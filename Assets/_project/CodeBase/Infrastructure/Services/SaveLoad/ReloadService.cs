using CodeBase.Infrastructure.State;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public class ReloadService : IReloadService
    {
        private readonly IGameStateMachine _gameStateMachine;

        public ReloadService(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public void Reload()
        {
            _gameStateMachine.Enter<BootstrapState>();
        }
    }
}
