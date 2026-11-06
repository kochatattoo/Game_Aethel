using CodeBase.Infrastructure.State;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public class ReloadService : IReloadService
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly CodeBase.CharacterIntegration.ICharacterMenuActions _menu;

        public ReloadService(IGameStateMachine gameStateMachine, CodeBase.CharacterIntegration.ICharacterMenuActions menu)
        {
            _gameStateMachine = gameStateMachine;
            _menu = menu;
        }

        public void Reload()
        {
            _gameStateMachine.Enter<ReloadGameState>();
        }
        public void NewGame() => _menu.StartNewGame();
    }
}
