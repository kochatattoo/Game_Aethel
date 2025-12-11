namespace CodeBase.Infrastructure.State
{
    /// <summary>
    /// Интерфейс для наших состояний
    /// </summary>
    public interface IState : IExitableState
    {
        /// <summary>
        /// Метод вызывающийся при входе в новое состояние
        /// </summary>
        void Enter();


    }

    public interface IExitableState
    {
        /// <summary>
        /// Метод выызвающийся при выходе из состояния
        /// </summary>
        void Exit();
    }

    public interface IPayloadedState<TPayload> : IExitableState
    {
        /// <summary>
        /// Метод вызывающийся при входе в новое состояние с передачей аргумента
        /// </summary>
        void Enter(TPayload payload);

    }
}
