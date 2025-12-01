using CodeBase.Infrastructure.State;

namespace CodeBase
{
    public class Game
    {
        private readonly IGameStateMachine _gameStateMachine;

        public Game(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
            _gameStateMachine.Enter<BootstrapState>();
        }
    }
}