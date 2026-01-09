using CodeBase.Infrastructure.State;
using Zenject;

namespace CodeBase.Infrastructure
{
    public class Game : IInitializable
    {
        private readonly IGameStateMachine _gameStateMachine;

        public Game(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public void Initialize()
        {
           CreateGame();
        }

        private void CreateGame()
        {
            _gameStateMachine.CreateGameStates();
            _gameStateMachine.Enter<BootstrapState>();
        }
    }
}
