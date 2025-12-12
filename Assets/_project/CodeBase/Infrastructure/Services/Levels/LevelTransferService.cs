using CodeBase.Infrastructure.State;
using Zenject;


namespace CodeBase.Infrastructure.Services.Levels
{
    public class LevelTransferService : ILevelTransferService
    {
        private IGameStateMachine _gameStateMachine;

        public LevelTransferService() { }

        public void Resolve(IGameStateMachine gameState)
        {
            _gameStateMachine = gameState;
        }

        public void GoTo(string levelName)
        {
            _gameStateMachine.Enter<LoadLevelState, string>(levelName);
        }
    }
}
