using CodeBase.Infrastructure.Factory;
using CodeBase.UI.Services.Factory;
using Cysharp.Threading.Tasks;
using CodeBase.CharacterIntegration;
using UnityEngine;

namespace CodeBase.Infrastructure.State
{
    public class WarmUpState : IState
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IUIFactory _uIFactory;
        private readonly IGameFactory _gameFactory;
        private readonly CharacterMenuService _menu;

        public WarmUpState(IGameStateMachine gameStateMachine, IUIFactory uIFactory, IGameFactory gameFactory, CharacterMenuService menu)
        {
            _gameStateMachine = gameStateMachine;
            _uIFactory = uIFactory;
            _gameFactory = gameFactory;
            _menu = menu;
        }

        public void Enter()
        {
            WarmUpAsync().Forget(_menu.LoadingFailed);
        }

        public void Exit(){ }

        private async UniTask WarmUpAsync()
        {
            var gameFactoryWarmUp = _gameFactory.WarmUpAsync();
            var uiFactoryCreateRoot = _uIFactory.CreateUIRootAsync();

            await UniTask.WhenAll(gameFactoryWarmUp, uiFactoryCreateRoot);

            _uIFactory.WarmUp();
            await _menu.InitializeAsync();
            _gameStateMachine.Enter<MainMenuState>();
        }
    }
}
