namespace CodeBase.Infrastructure.State
{
    public interface IGameStateMachine : IService
    {
        void CreateGameStates();
        void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>;
        void Enter<TState>() where TState : class, IState;
    }
}
