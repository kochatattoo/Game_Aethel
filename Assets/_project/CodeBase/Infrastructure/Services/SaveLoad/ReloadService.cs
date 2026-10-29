using CodeBase.Infrastructure.State;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public class ReloadService : IReloadService
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly CodeBase.Infrastructure.Factory.IServiceFactory _services;

        public ReloadService(IGameStateMachine gameStateMachine, CodeBase.Infrastructure.Factory.IServiceFactory services)
        {
            _gameStateMachine = gameStateMachine;
            _services = services;
        }

        public void Reload()
        {
            _gameStateMachine.Enter<ReloadGameState>();
        }
        public void NewGame() => _services.CreateService<CodeBase.CharacterIntegration.CharacterMenuService>().StartNewGame();
    }
}
