namespace CodeBase.Infrastructure.State
{
    public class GameLoopState : IState
    {
        private IGameStateMachine _stateMachine;

        public GameLoopState(IGameStateMachine gameStateMachine)
        {
            _stateMachine = gameStateMachine;
        }

        public void Enter()
        {

        }

        public void Exit()
        {

        }
    }
}