using CodeBase.Infrastructure.State;


namespace CodeBase.Infrastructure.Services.Levels
{
    public class LevelTransferService : ILevelTransferService
    {
        private readonly IGameStateMachine _gameStateMachine;

        public LevelTransferService(IGameStateMachine gameState)
        {
            _gameStateMachine = gameState;
        }

        public void GoTo(string levelName)
        {
            _gameStateMachine.Enter<LoadLevelState, string>(levelName);
        }

    }
}
