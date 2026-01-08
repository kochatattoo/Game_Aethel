using CodeBase.Infrastructure.Factory;
using CodeBase.UI.Services.Factory;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;


namespace CodeBase.Infrastructure.State
{
    public class WarmUpState : IState
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IUIFactory _uIFactory;
        private readonly IGameFactory _gameFactory;

        public WarmUpState(IGameStateMachine gameStateMachine, IUIFactory uIFactory, IGameFactory gameFactory)
        {
            _gameStateMachine = gameStateMachine;
            _uIFactory = uIFactory;
            _gameFactory = gameFactory;
        }

        public void Enter()
        {
            WarmUpAsync();
            _gameStateMachine.Enter<LoadProgressState>();
        }

        public void Exit()
        {

        }

        private async void  WarmUpAsync()
        {
            var gameFactoryWarmUp = _gameFactory.WarmUpAsync();
            var uiFactoryCreateRoot = _uIFactory.CreateUIRootAsync();

            await UniTask.WhenAll(gameFactoryWarmUp, uiFactoryCreateRoot);

            _uIFactory.WarmUp();
        }
    }
}
