using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Logic;
using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.State
{
    public class GameStateMachine: IGameStateMachine
    {
        private readonly Dictionary<Type, IExitableState> _states;

        private IExitableState _activeState;

        public GameStateMachine(SceneLoader sceneLoader, LoadingCurtain loadingCurtain, IGameFactory gameFactory, IStaticDataService staticDataService)
        {
            _states = new Dictionary<Type, IExitableState>() // Словарь наших стейтов ключ по типу
            {
                [typeof(BootstrapState)] = new BootstrapState(this, sceneLoader),
                [typeof(LoadLevelState)] = new LoadLevelState(this, 
                                                              loadingCurtain, sceneLoader, 
                                                              gameFactory, staticDataService),
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
