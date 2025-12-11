using CodeBase.Infrastructure.Factory;
using System;
using System.Collections.Generic;
using Zenject;

namespace CodeBase.Infrastructure.State
{
    public class GameStateMachine: IGameStateMachine, IInitializable
    {
        private readonly IStateFactory _stateFactory;

        private Dictionary<Type, IExitableState> _states;
        private IExitableState _activeState;

        public GameStateMachine(IStateFactory stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public void Initialize()
        {
            _states = new Dictionary<Type, IExitableState>
            {
                [typeof(BootstrapState)] = _stateFactory
                .CreateState<BootstrapState>(),
                [typeof(LoadLevelState)] = _stateFactory
                .CreateState<LoadLevelState>(),
                [typeof(GameLoopState)] = _stateFactory
                .CreateState<GameLoopState>()
            };

            Enter<BootstrapState>();
        }

        /// <summary>
        /// Входим в новое состояние через обобщеные типы (ограничение интерфейс IState)
        /// </summary>
        public void Enter<TState>() where TState : class, IState
        {
            IState state = ChangeState<TState>();
            state.Enter(); // Запускаем его
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
        {
            TState state = ChangeState<TState>();
            state.Enter(payload); // Запускаем его
        }

        private TState ChangeState<TState>() where TState : class, IExitableState
        {
            _activeState?.Exit();

            TState state = GetState<TState>();
            _activeState = state;

            return state;
        }

        private TState GetState<TState>() where TState : class, IExitableState =>
            _states[typeof(TState)] as TState;
    }

}
