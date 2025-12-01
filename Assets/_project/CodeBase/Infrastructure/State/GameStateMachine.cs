using CodeBase.Logic;
using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.State
{
    public class GameStateMachine: IGameStateMachine
    {
        private readonly Dictionary<Type, IExitableState> _states;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _loadingCurtain;

        private IExitableState _activeState;

        public GameStateMachine(SceneLoader sceneLoader, LoadingCurtain loadingCurtain)
        {
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;

            _states = new Dictionary<Type, IExitableState>() // Словарь наших стейтов ключ по типу
            {
                [typeof(BootstrapState)] = new BootstrapState(this, _sceneLoader),
                [typeof(LoadLevelState)] = new LoadLevelState(this, _loadingCurtain, _sceneLoader),
                [typeof(GameLoopState)] = new GameLoopState(this)
            };

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
